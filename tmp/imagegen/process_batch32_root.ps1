Add-Type -AssemblyName System.Drawing

$items = @(
    @{ Name = 'PriestVestment'; Source = 'C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-0508dce9-0c1c-44d8-8df5-62ad7b81494f.png' },
    @{ Name = 'RogueLeatherArmor'; Source = 'C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-fb16cd08-d2cf-4a00-9a2b-593dcc515a3d.png' }
)
$outDir = 'C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\UI\Codex\Equipment'
$sourceDir = 'C:\UnityProjects\DungeonMerchant\tmp\imagegen'

foreach ($item in $items) {
    $outPath = Join-Path $outDir ($item.Name + '.png')
    if (Test-Path -LiteralPath $outPath) { throw "Refusing to overwrite $outPath" }
    Copy-Item -LiteralPath $item.Source -Destination (Join-Path $sourceDir ($item.Name + '_source.png'))
    $src = [System.Drawing.Bitmap]::FromFile($item.Source)
    $keyed = New-Object System.Drawing.Bitmap $src.Width, $src.Height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $src.Height; $y++) {
        for ($x = 0; $x -lt $src.Width; $x++) {
            $c = $src.GetPixel($x,$y); $m = [Math]::Max($c.R,$c.B); $d = [Math]::Min($c.G-$c.R,$c.G-$c.B)
            $a = 255; $g = $c.G
            if ($c.G -gt 65 -and $d -gt 8) {
                $a = [Math]::Max(0,[Math]::Min(255,255-[int](($d-8)*255/42)))
                if ($a -lt 38) { $a = 0 }
                $g = [Math]::Min($c.G,$m)
            }
            $keyed.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($a,$c.R,$g,$c.B))
        }
    }
    $canvas = New-Object System.Drawing.Bitmap 256,256,([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $gfx = [System.Drawing.Graphics]::FromImage($canvas)
    $gfx.Clear([System.Drawing.Color]::Transparent)
    $gfx.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $gfx.DrawImage($keyed,10,10,236,236)
    $gfx.Dispose(); $keyed.Dispose(); $src.Dispose()
    $canvas.Save($outPath,[System.Drawing.Imaging.ImageFormat]::Png)
    $canvas.Dispose()
}
