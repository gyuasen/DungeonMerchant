Add-Type -AssemblyName System.Drawing
$sourcePath = 'C:\Users\yuga0\.codex\generated_images\019f7ad3-cbea-7592-b95f-7fce1cb21628\exec-a9501849-f68c-490a-a8fc-6ab3827e5cae.png'
$sourceCopy = 'C:\UnityProjects\DungeonMerchant\tmp\imagegen\TreasureCache_Careful-source.png'
$destination = 'C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Events'
$targetPath = Join-Path $destination 'TreasureCache_Careful.png'
if (Test-Path -LiteralPath $targetPath) { Remove-Item -LiteralPath $targetPath }
if (!(Test-Path -LiteralPath $destination)) { New-Item -ItemType Directory -Path $destination | Out-Null }
Copy-Item -LiteralPath $sourcePath -Destination $sourceCopy
$source = [System.Drawing.Bitmap]::FromFile($sourcePath)
try {
  $keyed = New-Object System.Drawing.Bitmap($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  for ($y = 0; $y -lt $source.Height; $y++) {
    for ($x = 0; $x -lt $source.Width; $x++) {
      $c = $source.GetPixel($x, $y)
      $dominance = [Math]::Min($c.G - $c.R, $c.G - $c.B)
      $a = 255; $g = $c.G
      if ($c.G -gt 70 -and $dominance -gt 8) {
        $a = [Math]::Max(0, [Math]::Min(255, 255 - (($dominance - 8) * 255 / 58)))
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
    $graphics.DrawImage($keyed, 8, 8, 496, 496)
  } finally { $graphics.Dispose() }
  for ($y = 0; $y -lt 512; $y++) {
    for ($x = 0; $x -lt 512; $x++) {
      $c = $canvas.GetPixel($x, $y)
      if ($c.A -gt 0 -and $c.G -gt 80 -and ([Math]::Min($c.G - $c.R, $c.G - $c.B)) -gt 20) {
        $canvas.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
      }
    }
  }
  $canvas.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)
  $canvas.Dispose(); $keyed.Dispose()
} finally { $source.Dispose() }
