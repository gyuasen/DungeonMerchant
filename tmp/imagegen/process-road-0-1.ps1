Add-Type -AssemblyName System.Drawing
$sourcePath = 'C:\Users\yuga0\.codex\generated_images\019f7ad3-cbea-7592-b95f-7fce1cb21628\exec-a195dec8-2f9f-4559-acb4-1b8ce6bfb248.png'
$sourceCopy = 'C:\UnityProjects\DungeonMerchant\tmp\imagegen\Road_0_1-source.png'
$targetPath = 'C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Backgrounds\Road_0_1.png'
if (Test-Path -LiteralPath $targetPath) { throw "Refusing to overwrite existing file: $targetPath" }
Copy-Item -LiteralPath $sourcePath -Destination $sourceCopy
$source = [System.Drawing.Bitmap]::FromFile($sourcePath)
try {
  $canvas = New-Object System.Drawing.Bitmap(1920, 1080, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
  $graphics = [System.Drawing.Graphics]::FromImage($canvas)
  try {
    $graphics.Clear([System.Drawing.Color]::Black)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.DrawImage($source, 0, 0, 1920, 1080)
  } finally { $graphics.Dispose() }
  $canvas.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)
  $canvas.Dispose()
} finally { $source.Dispose() }
