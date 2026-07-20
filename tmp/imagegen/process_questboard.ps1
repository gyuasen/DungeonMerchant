Add-Type -AssemblyName System.Drawing
$source='C:\Users\yuga0\.codex\generated_images\019f7a47-06d8-7b62-8373-138abfad0c65\exec-eb1f0393-44f5-41a4-bf5d-706e6b2fb150.png'
$sourceCopy='C:\UnityProjects\DungeonMerchant\tmp\imagegen\QuestBoard_source.png'
$out='C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\UI\QuestBoard.png'
Copy-Item $source $sourceCopy -Force
$src=[Drawing.Bitmap]::FromFile($source);$dst=New-Object Drawing.Bitmap $src.Width,$src.Height,([Drawing.Imaging.PixelFormat]::Format32bppArgb)
for($y=0;$y-lt$src.Height;$y++){for($x=0;$x-lt$src.Width;$x++){$c=$src.GetPixel($x,$y);$m=[Math]::Max($c.G,[Math]::Min($c.R,$c.B));$d=[Math]::Min($c.R-$c.G,$c.B-$c.G);$a=255;$r=$c.R;$b=$c.B;if($c.R-gt65-and$c.B-gt65-and$d-gt8){$a=[Math]::Max(0,[Math]::Min(255,255-[int](($d-8)*255/42)));if($a-lt38){$a=0};$r=[Math]::Min($r,$m);$b=[Math]::Min($b,$m)};$dst.SetPixel($x,$y,[Drawing.Color]::FromArgb($a,$r,$c.G,$b))}}
$src.Dispose()
$canvas=New-Object Drawing.Bitmap 1024,1024,([Drawing.Imaging.PixelFormat]::Format32bppArgb)
$gfx=[Drawing.Graphics]::FromImage($canvas);$gfx.Clear([Drawing.Color]::Transparent);$gfx.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$gfx.DrawImage($dst,0,0,1024,1024);$gfx.Dispose();$dst.Dispose()
for($y=0;$y-lt1024;$y++){for($x=0;$x-lt1024;$x++){$c=$canvas.GetPixel($x,$y);if($c.A-lt100){$canvas.SetPixel($x,$y,[Drawing.Color]::FromArgb(0,0,0,0))}else{$d=[Math]::Min($c.R-$c.G,$c.B-$c.G);if($d-gt4){$m=[Math]::Max($c.G,[Math]::Min($c.R,$c.B));$canvas.SetPixel($x,$y,[Drawing.Color]::FromArgb($c.A,[Math]::Min($c.R,$m),$c.G,[Math]::Min($c.B,$m)))}}}}
$canvas.Save($out,[Drawing.Imaging.ImageFormat]::Png);$canvas.Dispose()
