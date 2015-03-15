$currentPath = [Environment]::CurrentDirectory
$msBuildDir = $env:windir + "\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

$submission = "Submission"
$archive = "Archive.zip"

Try 
{
    $null = New-Item -ErrorAction Ignore -ItemType directory -Path Submission

    cd Src

    iex ($msBuildDir)

    cd bin\Debug

    .\QualificationTask.exe | Out-File -Encoding ascii output.txt

    Copy-Item output.txt $("..\..\..\" + $submission + "\")

    cd ..\..

    Remove-Item -Recurse -Force  "obj"
    Remove-Item -Recurse -Force  "bin"

    cd $("..\" + $submission)

    If (Test-Path $archive){
	    Remove-Item $archive
    }

    [Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")
    $Compression = [System.IO.Compression.CompressionLevel]::Optimal
    $IncludeBaseDirectory = $false

    $Source = $currentPath + "\Src\"
    $Destination = $currentPath + "\" + $submission + "\" + $archive

    [System.IO.Compression.ZipFile]::CreateFromDirectory($Source,$Destination,$Compression,$IncludeBaseDirectory)
}
Catch
{
    Write-Host $_.Exception.ToString() -ForegroundColor Red
}
    sleep 100000
