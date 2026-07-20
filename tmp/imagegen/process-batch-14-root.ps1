Add-Type -AssemblyName System.Drawing
$jobs = @(
 @{ Source='C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-82e5498d-72f4-4fc7-a702-7995f2e6534f.png'; Name='Grade02DemonKnight.png'; Size=512; Margin=18 },
 @{ Source='C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-d4f9ba48-0353-4a6c-a366-e060a5ce7593.png'; Name='Grade01AbyssDragon.png'; Size=768; Margin=20 }
)
$destination='C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Enemies'
foreach($job in $jobs){
 $target=Join-Path $destination $job.Name
 if(Test-Path -LiteralPath $target){throw "Refusing to overwrite existing file: $target"}
 $source=[System.Drawing.Bitmap]::FromFile($job.Source)
 try{
  $keyed=New-Object System.Drawing.Bitmap($source.Width,$source.Height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  for($y=0;$y-lt$source.Height;$y++){for($x=0;$x-lt$source.Width;$x++){
   $c=$source.GetPixel($x,$y);$d=[Math]::Min($c.G-$c.R,$c.G-$c.B);$a=255;$g=$c.G
   if($c.G-gt 90-and$d-gt 18){$a=[Math]::Max(0,[Math]::Min(255,255-(($d-18)*255/55)));if($a-lt 18){$a=0}elseif($a-lt 250){$g=[Math]::Min($c.G,[Math]::Max($c.R,$c.B))}}
   $keyed.SetPixel($x,$y,[System.Drawing.Color]::FromArgb([int]$a,$c.R,[int]$g,$c.B))
  }}
  $canvas=New-Object System.Drawing.Bitmap($job.Size,$job.Size,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $graphics=[System.Drawing.Graphics]::FromImage($canvas)
  try{$graphics.Clear([System.Drawing.Color]::Transparent);$graphics.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$draw=$job.Size-2*$job.Margin;$graphics.DrawImage($keyed,$job.Margin,$job.Margin,$draw,$draw)}finally{$graphics.Dispose()}
  $canvas.Save($target,[System.Drawing.Imaging.ImageFormat]::Png);$canvas.Dispose();$keyed.Dispose()
 }finally{$source.Dispose()}
}
