# dmx-simples
*Author: Bruno Di Prinzio de Oliveira*

[![Build Status](https://dev.azure.com/brunodpo/DMXSimples/_apis/build/status/DMXSimples-CI?branchName=master)](https://dev.azure.com/brunodpo/DMXSimples/_build/latest?definitionId=2&branchName=master)

### Description
C# implementation of the DMX-512 protocol (with a simple GUI)

### RS232 to RS485 Dongle
For this software to work, you must either buy or make a RS232 to RS485 dongle. There are many on sell today, both based on the Prolific IC or FTDI. As the DMX-512 Protocol uses a high baud rate (250kb/s), it's recommended that you choose the FTDI one as the Prolific-based can't reach such high values.

### TODOs
- Translate the UI to english