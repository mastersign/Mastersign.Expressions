$ErrorActionPreference = "Stop"
$base = [IO.Path]::Combine([IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Definition), "..");

function clean-dir($path)
{
    if (Test-Path $path)
    {
        rmdir $path -Recurse -Force
    }
    mkdir $path | Out-Null
    return $path
}

function version-from-assembly-info($assemblyInfoPath)
{
    $assemblyInfo = Get-Content $assemblyInfoPath | where { -not $_.Trim().StartsWith("//") }
    $assemblyInfo = [string]::Join([System.Environment]::NewLine, $assemblyInfo)
    $versionMatch = [regex]::Match($assemblyInfo, "\[assembly: AssemblyVersion\(`"(.+)`"\)\]")
    if (!$versionMatch.Success)
    {
        Write-Warning "No version information found in: $assemblyInfoPath."
        return "0.0.0"
    }
    $version = New-Object System.Version $versionMatch.Groups[1].Value
    return $version.ToString(3)
}

function copy-project-dir($path, $outDir)
{
    $outDir = [IO.Path]::Combine($outDir, [IO.Path]::GetFileName($path));
    Get-ChildItem $path `
        | where { $_.Name -ne "bin" -and $_.Name -ne "obj" } `
        | foreach { copy $_.FullName "$outDir\$($_.Name)" -Recurse }
}

function copy-solution($path, $name, $outDir)
{
    copy "$path\$name.sln" $outDir
    copy "$path\*.license.txt" $outDir
    copy "$path\packages" $outDir -Recurse
}

function pack-folder($path)
{
    if (Test-Path "$path.zip") { del "$path.zip" }
    7z a "$path.zip" "$path\*"
    if ($LASTEXITCODE)
    {
        Write-Error "7zip return error: $LASTEXITCODE"
    }
}

# Demo

$versionStr = version-from-assembly-info "$base\Mastersign.Expressions.Demo\Properties\AssemblyInfo.cs"
Write-Host "Building release for Mastersign.Expressions.Demo version $versionStr ..."

$demo_outDir = clean-dir "$base\releases\Mastersign.Expressions.Demo_v${versionStr}_bin"

copy "$base\*.license.txt" $demo_outDir
copy "$base\Mastersign.Expressions\bin\Release\Sprache.dll" $demo_outDir
copy "$base\Mastersign.Expressions\bin\Release\Mastersign.Expressions.dll" $demo_outDir
copy "$base\Mastersign.Expressions.Demo\bin\Release\Mastersign.Expressions.Demo.exe*" $demo_outDir

pack-folder $demo_outDir

# Binaries

$versionStr = version-from-assembly-info "$base\Mastersign.Expressions\Properties\AssemblyInfo.cs"
Write-Host "Building release for Mastersign.Expressions version $versionStr ..."

$bin_outDir = clean-dir "$base\releases\Mastersign.Expressions_v${versionStr}_bin"

copy "$base\*.license.txt" $bin_outDir
copy "$base\Mastersign.Expressions\bin\Release\Sprache.dll" $bin_outDir
copy "$base\Mastersign.Expressions\bin\Release\Mastersign.Expressions.*" $bin_outDir

pack-folder $bin_outDir

# Sources

$src_outDir = clean-dir "$base\releases\Mastersign.Expressions_v${versionStr}_src"

copy-solution $base "Mastersign.Expressions" $src_outDir
copy-project-dir "$base\Mastersign.Expressions" $src_outDir
copy-project-dir "$base\Mastersign.Expressions.Demo" $src_outDir

pack-folder $src_outDir