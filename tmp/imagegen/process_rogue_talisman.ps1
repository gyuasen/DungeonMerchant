Add-Type -AssemblyName System.Drawing
$src='C:\Users\yuga0\.codex\generated_images\019f7ad3-5973-74a2-ae4b-3fb04ce9cb71\exec-367376e6-72cd-4f75-9393-d8c41e954f38.png'
$source='C:\UnityProjects\DungeonMerchant\tmp\imagegen\RogueTalisman_source.png'
$final='C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\UI\Codex\Equipment\RogueTalisman.png'
$review='C:\UnityProjects\DungeonMerchant\tmp\review\RogueTalisman_gray.png'
$thumb='C:\UnityProjects\DungeonMerchant\tmp\review\RogueTalisman_64.png'
New-Item -ItemType Directory -Force (Split-Path $source),(Split-Path $review),(Split-Path $final) | Out-Null
Copy-Item -LiteralPath $src -Destination $source -Force
$input=[System.Drawing.Bitmap]::FromFile($source)
$scaled=[System.Drawing.Bitmap]::new(256,256,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[System.Drawing.Graphics]::FromImage($scaled);$g.Clear([System.Drawing.Color]::Lime);$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality;$g.DrawImage($input,[System.Drawing.Rectangle]::new(16,16,224,224));$g.Dispose();$input.Dispose()
$out=[System.Drawing.Bitmap]::new(256,256,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for($y=0;$y-lt256;$y++){for($x=0;$x-lt256;$x++){$c=$scaled.GetPixel($x,$y);$r=[double]$c.R;$gg=[double]$c.G;$b=[double]$c.B;$d=[Math]::Sqrt($r*$r+(255-$gg)*(255-$gg)+$b*$b);$a=[Math]::Max(0,[Math]::Min(255,($d-18)*255/72));$rr=$r;$gb=$gg;$bb=$b;$dom=$gg-[Math]::Max($r,$b);if($gg-gt70-and$dom-gt18){$m=[Math]::Max($r,$b);$matte=[Math]::Max(0,[Math]::Min(255,((255-$gg)+(0.35*$m))*255/175));$a=[Math]::Min($a,$matte);$gb=[Math]::Min($gg,1.03*$m);if($gg-gt215-and$dom-gt120){$a=0}};if($a-lt8){$a=0};$out.SetPixel($x,$y,[System.Drawing.Color]::FromArgb([int]$a,[int]$rr,[int]$gb,[int]$bb))}}
$scaled.Dispose();$out.Save($final,[System.Drawing.Imaging.ImageFormat]::Png)
$gray=[System.Drawing.Bitmap]::new(256,256,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$cg=[System.Drawing.Graphics]::FromImage($gray);$cg.Clear([System.Drawing.Color]::FromArgb(35,35,35));$cg.DrawImage($out,0,0);$cg.Dispose();$gray.Save($review,[System.Drawing.Imaging.ImageFormat]::Png)
$small=[System.Drawing.Bitmap]::new(64,64,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$sg=[System.Drawing.Graphics]::FromImage($small);$sg.Clear([System.Drawing.Color]::FromArgb(35,35,35));$sg.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$sg.DrawImage($out,[System.Drawing.Rectangle]::new(0,0,64,64));$sg.Dispose();$small.Save($thumb,[System.Drawing.Imaging.ImageFormat]::Png)
$minX=256;$minY=256;$maxX=-1;$maxY=-1;$count=0;$green=0;for($y=0;$y-lt256;$y++){for($x=0;$x-lt256;$x++){$c=$out.GetPixel($x,$y);if($c.A-gt16){$count++;if($x-lt$minX){$minX=$x};if($x-gt$maxX){$maxX=$x};if($y-lt$minY){$minY=$y};if($y-gt$maxY){$maxY=$y};if($c.G-gt100-and$c.G-gt1.5*$c.R-and$c.G-gt1.5*$c.B){$green++}}}}
$corners=@($out.GetPixel(0,0).A,$out.GetPixel(255,0).A,$out.GetPixel(0,255).A,$out.GetPixel(255,255).A);Write-Output "Size=$($out.Width)x$($out.Height) Format=$($out.PixelFormat) Bounds=$minX,$minY-$maxX,$maxY Coverage=$([Math]::Round(100*$count/65536,2))% Corners=$($corners-join',') GreenCandidates=$green";$out.Dispose();$gray.Dispose();$small.Dispose()
