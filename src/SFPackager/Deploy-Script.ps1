$NewVersion = ##VERSION##;
$BasePackagePath = ##BASEPATH##;
$Packages = ##PACKAGES##;

$Suffix = Get-Random -Maximum 99999
try
{
	Write-Information "Connecting to the cluster..."
	[void](Connect-ServiceFabricCluster)
}
catch
{
	Write-Warning "Unable to connect to local cluster!"
	throw
}

foreach($package in $Packages) {
	$packagePath = Join-Path -Path $BasePackagePath -ChildPath $package
	$packageName = $package + $Suffix

	Write-Information "Testing application package $package"
	$packageValidation = (Test-ServiceFabricApplicationPackage -ApplicationPackagePath $packagePath -ImageStoreConnectionString fabric:ImageStore)
	if(!$packageValidation) {
		throw "Validation failed for package: $package"
	}

	Write-Information "Copying application package $package"
	Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $packagePath -ImageStoreConnectionString fabric:ImageStore -ApplicationPackagePathInImageStore $packageName

	Register-ServiceFabricApplicationType -ApplicationPathInImageStore $packageName -TimeoutSec 600
}

$apps = Get-ServiceFabricApplication
foreach($app in $apps) {
	Write-Information "Starting upgrade of $app"
	[void](Start-ServiceFabricApplicationUpgrade -ApplicationName $app.ApplicationName -ApplicationTypeVersion $NewVersion -UnmonitoredAuto)
}