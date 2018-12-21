# This script is used during the build process to automatically push to VirusTotal for analysis.
# Adapted from script originally available here: https://psvirustotal.codeplex.com
param(
    [String]$VirusTotalApiKey,
    [String]$FileUri,
    [String]$FilePath
)

Process {
    If($FilePath) {
        $FileInfo = ([System.IO.FileInfo]$FilePath)

        $output = Invoke-VTScan -VTApiKey $VirusTotalApiKey -file $FileInfo
    }
    Else {
        $output = Invoke-VTScan -VTApiKey $VirusTotalApiKey -file $FileUri
    }

    Set-VstsVariable "scan_id" $output.scan_id
    Set-VstsVariable "sha1" $output.sha1
    Set-VstsVariable "resource" $output.resource
    Set-VstsVariable "response_code" $output.response_code
    Set-VstsVariable "sha256" $output.sha256
    Set-VstsVariable "permalink" $output.permalink
    Set-VstsVariable "md5" $output.md5
    Set-VstsVariable "verbose_msg" $output.verbose_msg
}

Begin {
    function Set-VTApiKey {
        [CmdletBinding()]
        Param([Parameter(Mandatory=$true)][ValidateNotNull()][String] $VTApiKey,
        [String] $vtFileLocation = $(Join-Path $env:APPDATA 'virustotal.bin'))
        $inBytes = [System.Text.Encoding]::Unicode.GetBytes($VTApiKey)
        $protected = [System.Security.Cryptography.ProtectedData]::Protect($inBytes, $null, [System.Security.Cryptography.DataProtectionScope]::CurrentUser)
        [System.IO.File]::WriteAllBytes($vtfileLocation, $protected)
    }

    function Get-VTApiKey {
        [CmdletBinding()]
        Param([String] $vtFileLocation = $(Join-Path $env:APPDATA 'virustotal.bin'))
        if (Test-Path $vtfileLocation) {
            $protected = [System.IO.File]::ReadAllBytes($vtfileLocation)
            $rawKey = [System.Security.Cryptography.ProtectedData]::Unprotect($protected, $null, [System.Security.Cryptography.DataProtectionScope]::CurrentUser)
            return [System.Text.Encoding]::Unicode.GetString($rawKey)
        } else {
            throw "Call Set-VTApiKey first!"
        }
    }

    function Get-VTReport {
        [CmdletBinding()]
        Param(
        [String] $VTApiKey = (Get-VTApiKey),
        [Parameter(ParameterSetName="hash", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][String] $hash,
        [Parameter(ParameterSetName="file", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][System.IO.FileInfo] $file,
        [Parameter(ParameterSetName="uri", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][Uri] $uri,
        [Parameter(ParameterSetName="ipaddress", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][String] $ip,
        [Parameter(ParameterSetName="domain", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][String] $domain
        )
        Begin {
            $fileUri = 'https://www.virustotal.com/vtapi/v2/file/report'
            $UriUri = 'https://www.virustotal.com/vtapi/v2/url/report'
            $IPUri = 'http://www.virustotal.com/vtapi/v2/ip-address/report'
            $DomainUri = 'http://www.virustotal.com/vtapi/v2/domain/report'

            function Get-Hash(
                [System.IO.FileInfo] $file = $(Throw 'Usage: Get-Hash [System.IO.FileInfo]'),
                [String] $hashType = 'sha256')
            {
              $stream = $null;
              [string] $result = $null;
              $hashAlgorithm = [System.Security.Cryptography.HashAlgorithm]::Create($hashType )
              $stream = $file.OpenRead();
              $hashByteArray = $hashAlgorithm.ComputeHash($stream);
              $stream.Close();

              trap
              {
                if ($stream -ne $null) { $stream.Close(); }
                break;
              }

              # Convert the hash to Hex
              $hashByteArray | foreach { $result += $_.ToString("X2") }
              return $result
            }
        }
        Process {
            [String] $h = $null
            [String] $u = $null
            [String] $method = $null
            $body = @{}

            switch ($PSCmdlet.ParameterSetName) {
            "file" {
                $h = Get-Hash -file $file
                Write-Verbose -Message ("FileHash:" + $h)
                $u = $fileUri
                $method = 'POST'
                $body = @{ resource = $h; apikey = $VTApiKey}
                }
            "hash" {
                $u = $fileUri
                $method = 'POST'
                $body = @{ resource = $hash; apikey = $VTApiKey}
                }
            "uri" {
                $u = $UriUri
                $method = 'POST'
                $body = @{ resource = $uri; apikey = $VTApiKey}
                }
            "ipaddress" {
                $u = $IPUri
                $method = 'GET'
                $body = @{ ip = $ip; apikey = $VTApiKey}
            }
            "domain" {
                $u = $DomainUri
                $method = 'GET'
                $body = @{ domain = $domain; apikey = $VTApiKey}}
            }

            return Invoke-RestMethod -Method $method -Uri $u -Body $body
        }
    }

    function Invoke-VTScan {
        [CmdletBinding()]
        Param(
        [String] $VTApiKey = (Get-VTApiKey),
        [Parameter(ParameterSetName="file", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)]
            [System.IO.FileInfo] $file,
        [Parameter(ParameterSetName="uri", ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)]
            [Uri] $uri
        )
        Begin {
            $fileUri = 'https://www.virustotal.com/vtapi/v2/file/scan'
            $UriUri = 'https://www.virustotal.com/vtapi/v2/url/scan'
            [byte[]]$CRLF = 13, 10

            function Get-AsciiBytes([String] $str) {
                return [System.Text.Encoding]::ASCII.GetBytes($str)
            }
        }
        Process {
            [String] $h = $null
            [String] $u = $null
            [String] $method = $null
            $body = New-Object System.IO.MemoryStream

            switch ($PSCmdlet.ParameterSetName) {
            "file" {
                $u = $fileUri
                $method = 'POST'
                $boundary = [Guid]::NewGuid().ToString().Replace('-','')
                $ContentType = 'multipart/form-data; boundary=' + $boundary
                $b2 = Get-AsciiBytes ('--' + $boundary)
                $body.Write($b2, 0, $b2.Length)
                $body.Write($CRLF, 0, $CRLF.Length)

                $b = (Get-AsciiBytes ('Content-Disposition: form-data; name="apikey"'))
                $body.Write($b, 0, $b.Length)

                $body.Write($CRLF, 0, $CRLF.Length)
                $body.Write($CRLF, 0, $CRLF.Length)

                $b = (Get-AsciiBytes $VTApiKey)
                $body.Write($b, 0, $b.Length)

                $body.Write($CRLF, 0, $CRLF.Length)
                $body.Write($b2, 0, $b2.Length)
                $body.Write($CRLF, 0, $CRLF.Length)

                $b = (Get-AsciiBytes ('Content-Disposition: form-data; name="file"; filename="' + $file.Name + '";'))
                $body.Write($b, 0, $b.Length)
                $body.Write($CRLF, 0, $CRLF.Length)
                $b = (Get-AsciiBytes 'Content-Type:application/octet-stream')
                $body.Write($b, 0, $b.Length)

                $body.Write($CRLF, 0, $CRLF.Length)
                $body.Write($CRLF, 0, $CRLF.Length)

                $b = [System.IO.File]::ReadAllBytes($file.FullName)
                $body.Write($b, 0, $b.Length)

                $body.Write($CRLF, 0, $CRLF.Length)
                $body.Write($b2, 0, $b2.Length)

                $b = (Get-AsciiBytes '--')
                $body.Write($b, 0, $b.Length)

                $body.Write($CRLF, 0, $CRLF.Length)

                Invoke-RestMethod -Method $method -Uri $u -ContentType $ContentType -Body $body.ToArray()
                }
            "uri" {
                $h = $uri
                $u = $UriUri
                $method = 'POST'
                $body = @{ url = $uri; apikey = $VTApiKey}
                Invoke-RestMethod -Method $method -Uri $u -Body $body
                }
            }
        }
    }

    function New-VTComment {
        [CmdletBinding()]
        Param(
        [String] $VTApiKey = (Get-VTApiKey),
        [Parameter(Mandatory=$true,ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][String] $hash,
        [Parameter(Mandatory=$true)][ValidateNotNull()][String] $Comment
        )

        Process {
            $u = 'https://www.virustotal.com/vtapi/v2/comments/put'
            $method = 'POST'
            $body = @{ resource = $hash; apikey = $VTApiKey; comment = $Comment}

            return Invoke-RestMethod -Method $method -Uri $u -Body $body
        }
    }

    function Invoke-VTRescan {
     [CmdletBinding()]
        Param(
        [String] $VTApiKey = (Get-VTApiKey),
        [Parameter(Mandatory=$true, ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][String] $hash
        )
        Process {
            $u = 'https://www.virustotal.com/vtapi/v2/file/rescan'
            $method = 'POST'
            $body = @{ resource = $hash; apikey = $VTApiKey}
            return Invoke-RestMethod -Method $method -Uri $u -Body $body
        }
    }

    function Set-VstsVariable {
        param([String]$variablename, [String]$variablevalue, [Bool]$secret = $false)
        Write-Output ("##vso[task.setvariable variable=$variablename;isSecret=$secret;isOutput=true;] $variablevalue")
    }
}