Import-Module KqlPowerShell\KqlPowerShell.dll

function Get-EtwRealTimeKql {
    param(
        # Etw Session to attach to
        [Parameter(Mandatory, Position = 0)]
        [string]
        $SessionName,

        # Query to use
        [Parameter()]
        [AllowEmptyString()]
        [string]
        $Query
        )

    Get-KqlPowerShell "etw" $SessionName -Query $Query
}

function Get-EtlFileReaderKql {
    param(
        # Etl file to read
        [Parameter(Mandatory, Position = 0)]
        [string]
        $FileName,

        # Query to use
        [Parameter()]
        [AllowEmptyString()]
        [string]
        $Query
        )
    
    Get-KqlPowerShell "etl" $FileName -Query $Query
}

Export-ModuleMember -Function Get-EtwRealTimeKql, Get-EtlFileReaderKql