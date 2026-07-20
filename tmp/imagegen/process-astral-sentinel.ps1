Add-Type -AssemblyName System.Drawing
$sourcePath = 'C:\Users\yuga0\.codex\generated_images\019f7ad3-cbea-7592-b95f-7fce1cb21628\exec-39038b59-efdd-49f4-96d2-be00a31b49cf.png'
$sourceCopy = 'C:\UnityProjects\DungeonMerchant\tmp\imagegen\Grade01AstralSentinel-source.png'
$targetPath = 'C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Enemies\Grade01AstralSentinel.png'
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
  $canvas = New-Object System.Drawing.Bitmap(768, 768, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $graphics = [System.Drawing.Graphics]::FromImage($canvas)
  try {
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($keyed, 10, 10, 748, 748)
  } finally { $graphics.Dispose() }
  $canvas.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)
  $canvas.Dispose(); $keyed.Dispose()
} finally { $source.Dispose() }
