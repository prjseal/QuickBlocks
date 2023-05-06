$dir = Join-Path (Split-Path -Path $MyInvocation.MyCommand.Path) 'umbraco\Data\'
#Write-Output "Target directory: $dir"
Get-ChildItem -Path $dir -Filter 'Umbraco.sqlite*' | ForEach-Object {
    Write-Host "Deleted: $($_.Name)" -ForegroundColor Green
    Remove-Item $_.FullName -Force
}

$dir = Join-Path (Split-Path -Path $MyInvocation.MyCommand.Path) 'Views\Partials\blocklist\Components\'
#Write-Output "Target directory: $dir"
Get-ChildItem -Path $dir -Filter '*.cshtml' | ForEach-Object {
    Write-Host "Deleted: $($_.Name)" -ForegroundColor Green
    Remove-Item $_.FullName -Force
}

$dir = Join-Path (Split-Path -Path $MyInvocation.MyCommand.Path) 'Views\'
#Write-Output "Target directory: $dir"

Get-ChildItem -Path $dir -File -Filter '*' -Depth 0 | Where-Object {$_.Name -ne '_ViewImports.cshtml'} | ForEach-Object {
    Write-Host "Deleted: $($_.Name)" -ForegroundColor Green
    Remove-Item $_.FullName -Force
}

$dir = Join-Path (Split-Path -Path $MyInvocation.MyCommand.Path) 'Views\Partials\'
#Write-Output "Target directory: $dir"

Get-ChildItem -Path $dir -File -Filter '*' -Depth 0 | Where-Object {$_.Name -ne '_ViewImports.cshtml'} | ForEach-Object {
    Write-Host "Deleted: $($_.Name)" -ForegroundColor Green
    Remove-Item $_.FullName -Force
}