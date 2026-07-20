Add-Type -AssemblyName System.Drawing
$sourcePath = 'C:\Users\yuga0\.codex\generated_images\019f7ad3-cbea-7592-b95f-7fce1cb21628\exec-de5a173c-5208-407a-92cb-e7ad4695984c.png'
$sourceCopy = 'C:\UnityProjects\DungeonMerchant\tmp\imagegen\NormalRank04Armor-source.png'
$targetPath = 'C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\UI\Codex\Equipment\NormalRank04Armor.png'
if (Test-Path -LiteralPath $targetPath) { Remove-Item -LiteralPath $targetPath }
Copy-Item -LiteralPath $sourcePath -Destination $sourceCopy
$source = [System.Drawing.Bitmap]::FromFile($sourcePath)
try {
  $keyed = New-Object System.Drawing.Bitmap($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  for ($y = 0; $y -lt $source.Height; $y++) {
    for ($x = 0; $x -lt $source.Width; $x++) {
      $c = $source.GetPixel($x, $y)
      $dominance = [Math]::Min($c.R - $c.G, $c.B - $c.G)
      $a = 255; $r = $c.R; $b = $c.B
      if ($c.R -gt 70 -and $c.B -gt 70 -and $dominance -gt 8) {
        $a = [Math]::Max(0, [Math]::Min(255, 255 - (($dominance - 8) * 255 / 58)))
        if ($a -lt 18) { $a = 0 }
        elseif ($a -lt 250) { $r = [Math]::Min($c.R, $c.G); $b = [Math]::Min($c.B, $c.G) }
      }
      $keyed.SetPixel($x, $y, [System.Drawing.Color]::FromArgb([int]$a, [int]$r, $c.G, [int]$b))
    }
  }
  $canvas = New-Object System.Drawing.Bitmap(256, 256, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $graphics = [System.Drawing.Graphics]::FromImage($canvas)
  try {
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($keyed, 5, 5, 246, 246)
  } finally { $graphics.Dispose() }
  for ($y = 0; $y -lt 256; $y++) {
    for ($x = 0; $x -lt 256; $x++) {
      $c = $canvas.GetPixel($x, $y)
      if ($c.A -gt 0) {
        $d = [Math]::Min($c.R - $c.G, $c.B - $c.G)
        if ($d -gt 4) {
          $canvas.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($c.A, $c.G, $c.G, $c.G))
        }
      }
    }
  }
  $canvas.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)
  $canvas.Dispose(); $keyed.Dispose()
} finally { $source.Dispose() }
