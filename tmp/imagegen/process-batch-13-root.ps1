Add-Type -AssemblyName System.Drawing

$jobs = @(
    @{ Source = 'C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-0e75cf63-3eeb-43b4-b8fa-7b16a189f77b.png'; Name = 'enemy_job_wyvern_captain.png' },
    @{ Source = 'C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-9ccd1688-0711-421e-8ca8-6d95d369d1cf.png'; Name = 'Grade05IronGolem.png' }
)
$destination = 'C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Enemies'

foreach ($job in $jobs) {
    $target = Join-Path $destination $job.Name
    if (Test-Path -LiteralPath $target) { throw "Refusing to overwrite existing file: $target" }
    $source = [System.Drawing.Bitmap]::FromFile($job.Source)
    try {
        $keyed = New-Object System.Drawing.Bitmap($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $c = $source.GetPixel($x, $y)
                $d = [Math]::Min($c.G - $c.R, $c.G - $c.B)
                $a = 255
                $g = $c.G
                if ($c.G -gt 90 -and $d -gt 18) {
                    $a = [Math]::Max(0, [Math]::Min(255, 255 - (($d - 18) * 255 / 55)))
                    if ($a -lt 18) { $a = 0 }
                    elseif ($a -lt 250) { $g = [Math]::Min($c.G, [Math]::Max($c.R, $c.B)) }
                }
                $keyed.SetPixel($x, $y, [System.Drawing.Color]::FromArgb([int]$a, $c.R, [int]$g, $c.B))
            }
        }
        $canvas = New-Object System.Drawing.Bitmap(512, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($canvas)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $size = 476
            $graphics.DrawImage($keyed, 18, 18, $size, $size)
        } finally { $graphics.Dispose() }
        $canvas.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
        $canvas.Dispose()
        $keyed.Dispose()
    } finally { $source.Dispose() }
}
