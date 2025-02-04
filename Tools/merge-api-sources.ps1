param(
    [Parameter(Mandatory)] [string] $SourceDir,
    [Parameter(Mandatory)] [string] $OutDir
)

$sources = @(
    'Api/IStarControlApi.cs',
    'Menus/IRadialMenuPage.cs',
    'Menus/IRadialMenuItem.cs',
    'DelayedActions.cs',
    'Menus/ItemActivationType.cs',
    'Menus/ItemActivationResult.cs'
)

$usingLines = [System.Collections.Generic.List[string]]::new()
$codeLines = [System.Collections.Generic.List[string]]::new()

foreach ($source in $sources) {
    $path = Join-Path -Path $SourceDir -ChildPath $source
    $lines = Get-Content -Path $path
    $reachedNamespace = $false
    foreach ($line in $lines)
    {
        if ($line.StartsWith("using StarControl")) {
            continue
        }
        if ($line.StartsWith("namespace")) {
            $reachedNamespace = $true
            continue
        }
        if ($line.StartsWith("using")) {
            if (!$usingLines.Contains($line)) {
                $usingLines.Add($line)
            }
        } elseif ($reachedNamespace) {
            $codeLines.Add("    " + $line)
        }
    }
}

$usingLines.Sort()
$outLines = [System.Collections.Generic.List[string]]::new($usingLines)
$outLines.Add("")
$outLines.Add("namespace StarControl")
$outLines.Add("{")
$outLines.AddRange($codeLines)
$outLines.Add("}")
$outPath = Join-Path -Path $OutDir -ChildPath "IStarControlApi.cs"
Set-Content -Path $outPath -Value $outLines