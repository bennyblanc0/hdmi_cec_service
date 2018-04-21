# hdmi_cec_service
Service code to listen for TCP requests and forward them on to a HDMI CEC device.

## Install
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe hdmi_cec_service.exe

## Configure
`sc config hdmi_cec_service binpath="<path_to_exe>\hdmi_cec_service.exe <arg1> <arg2> <arg3>"`
