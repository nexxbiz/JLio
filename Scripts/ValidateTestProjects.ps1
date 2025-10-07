# Script to validate test project configurations and prevent duplicate test container issues

Write-Host "Validating test project configurations..." -ForegroundColor Green

$solution = Get-ChildItem -Filter "*.sln" | Select-Object -First 1
if (-not $solution) {
    Write-Error "No solution file found"
    exit 1
}

# Get all test projects
$testProjects = Get-ChildItem -Recurse -Filter "*.csproj" | Where-Object { 
    $content = Get-Content $_.FullName -Raw
    $content -match "<IsTestProject>true</IsTestProject>" -or 
    $content -match "Microsoft\.NET\.Test\.Sdk" -or
    $_.Name -like "*Test*" -or 
    $_.Name -like "*Tests*"
}

Write-Host "Found $($testProjects.Count) test projects:" -ForegroundColor Yellow
$testProjects | ForEach-Object { Write-Host "  - $($_.Name)" }

# Check for duplicate assembly names
$assemblyNames = @{}
$issues = @()

foreach ($project in $testProjects) {
    $content = Get-Content $project.FullName -Raw
    
    # Extract assembly name (default is project name without extension)
    $assemblyName = $project.BaseName
    if ($content -match "<AssemblyName>([^<]+)</AssemblyName>") {
        $assemblyName = $matches[1]
    }
    
    # Check for duplicates
    if ($assemblyNames.ContainsKey($assemblyName)) {
        $issues += "Duplicate assembly name '$assemblyName' found in:"
        $issues += "  - $($assemblyNames[$assemblyName])"
        $issues += "  - $($project.FullName)"
    } else {
        $assemblyNames[$assemblyName] = $project.FullName
    }
    
    # Extract Microsoft.NET.Test.Sdk version
    if ($content -match 'Microsoft\.NET\.Test\.Sdk["\s]*Version\s*=\s*["\s]*([^">\s]+)') {
        $testSdkVersion = $matches[1]
        Write-Host "  $($project.Name): Microsoft.NET.Test.Sdk v$testSdkVersion" -ForegroundColor Cyan
    }
    
    # Check for IsTestProject property
    if ($content -notmatch "<IsTestProject>true</IsTestProject>") {
        $issues += "Missing <IsTestProject>true</IsTestProject> in $($project.Name)"
    }
}

# Report issues
if ($issues.Count -gt 0) {
    Write-Host "`nIssues found:" -ForegroundColor Red
    $issues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
} else {
    Write-Host "`nAll test projects are properly configured!" -ForegroundColor Green
}

# Check for stale test assemblies
Write-Host "`nChecking for duplicate test assemblies in bin/obj folders..." -ForegroundColor Yellow
$testDlls = Get-ChildItem -Recurse -Filter "*.Tests.dll" -ErrorAction SilentlyContinue
$testDlls += Get-ChildItem -Recurse -Filter "*Test*.dll" -ErrorAction SilentlyContinue | Where-Object { $_.Name -notlike "*TestPlatform*" -and $_.Name -notlike "*TestHost*" }

$dllGroups = $testDlls | Group-Object Name
$duplicateDlls = $dllGroups | Where-Object Count -gt 3  # bin, obj, ref typically

if ($duplicateDlls) {
    Write-Host "Potential duplicate test assemblies found:" -ForegroundColor Red
    $duplicateDlls | ForEach-Object {
        Write-Host "  $($_.Name) ($($_.Count) copies)" -ForegroundColor Red
        $_.Group | ForEach-Object { Write-Host "    $($_.FullName)" -ForegroundColor Gray }
    }
    Write-Host "Consider running 'dotnet clean' to remove stale assemblies" -ForegroundColor Yellow
}

Write-Host "`nValidation complete!" -ForegroundColor Green