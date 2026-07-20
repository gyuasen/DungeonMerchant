Add-Type -AssemblyName System.Drawing
$path='C:\UnityProjects\DungeonMerchant\Assets\Proiject\Resources\UI\Codex\Equipment\ShadowhideArmor.png'
$src=[Drawing.Bitmap]::FromFile($path)
$dst=New-Object Drawing.Bitmap $src.Width,$src.Height,([Drawing.Imaging.PixelFormat]::Format32bppArgb)
for($y=0;$y-lt$src.Height;$y++){for($x=0;$x-lt$src.Width;$x++){$c=$src.GetPixel($x,$y);$g=$c.G;$m=[Math]::Max($c.R,$c.B);if($g-$m-gt4){$g=$m};$dst.SetPixel($x,$y,[Drawing.Color]::FromArgb($c.A,$c.R,$g,$c.B))}}
$src.Dispose();$dst.Save($path,[Drawing.Imaging.ImageFormat]::Png);$dst.Dispose()
