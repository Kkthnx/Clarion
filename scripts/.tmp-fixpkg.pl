use strict; use warnings;
local $/;
my $f = "scripts/make-package-manifests.ps1";
open my $h, '<', $f or die; binmode $h; my $s = <$h>; close $h;
my $crlf = $s =~ /\r\n/; $s =~ s/\r\n/\n/g;
$s =~ s/\$dir = Join-Path \$root "\$OutDir\winget\manifests\k\Kkthnx\Clarion\\$Version"/\$outRoot = if ([IO.Path]::IsPathRooted(\$OutDir)) { \$OutDir } else { Join-Path \$root \$OutDir }\n\$dir = Join-Path \$outRoot "winget\manifests\k\Kkthnx\Clarion\\$Version"/ or die 1;
$s =~ s/\$scoopDir = Join-Path \$root "\$OutDir\scoop"/\$scoopDir = Join-Path \$outRoot "scoop"/ or die 2;
$s =~ s/\n/\r\n/g if $crlf;
open my $o, '>', $f or die; binmode $o; print $o $s; close $o; print "ok\n";
