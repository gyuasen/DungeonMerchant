Add-Type -AssemblyName System.Drawing
$jobs=@(
 @{Source='C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-82e5498d-72f4-4fc7-a702-7995f2e6534f.png';Target='C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Enemies\Grade02DemonKnight.png';Size=512;Margin=18},
 @{Source='C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-d4f9ba48-0353-4a6c-a366-e060a5ce7593.png';Target='C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\Battle\Enemies\Grade01AbyssDragon.png';Size=768;Margin=20}
)
foreach($job in $jobs){
 $src=[System.Drawing.Bitmap]::FromFile($job.Source)
 try{
  $key=New-Object System.Drawing.Bitmap($src.Width,$src.Height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  for($y=0;$y-lt$src.Height;$y++){for($x=0;$x-lt$src.Width;$x++){
   $c=$src.GetPixel($x,$y);$d=[Math]::Min($c.G-$c.R,$c.G-$c.B);$a=255;$r=$c.R;$g=$c.G;$b=$c.B
   if($c.G-gt 70-and$d-gt 10){$a=[Math]::Max(0,[Math]::Min(255,255-(($d-10)*255/45)));if($a-lt 32){$a=0}else{$g=[Math]::Min($g,[Math]::Max($r,$b));if($a-lt 230){$a=[Math]::Max(0,$a-28)}}}
   $key.SetPixel($x,$y,[System.Drawing.Color]::FromArgb([int]$a,$r,[int]$g,$b))
  }}
  $canvas=New-Object System.Drawing.Bitmap($job.Size,$job.Size,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$gr=[System.Drawing.Graphics]::FromImage($canvas)
  try{$gr.Clear([System.Drawing.Color]::Transparent);$gr.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$draw=$job.Size-2*$job.Margin;$gr.DrawImage($key,$job.Margin,$job.Margin,$draw,$draw)}finally{$gr.Dispose()}
  $canvas.Save($job.Target,[System.Drawing.Imaging.ImageFormat]::Png);$canvas.Dispose();$key.Dispose()
 }finally{$src.Dispose()}
}
