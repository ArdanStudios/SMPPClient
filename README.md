SMPPClient
==========

**For use new featuers and bugfixes see https://github.com/BSVN/SMPPClient**

Copyright 2011-2013 Ardan Studios. All rights reserved.<br />
Use of this source code is governed by a BSD-style license that can be found in the LICENSE handle.

SMPP Client in C#

This class library implements the SMPP 3.4 protocol for use within .Net application. It can be used to build both
ESME and SMSC based software.

Key Features<br />
1. Fully tested production code. Can handle millions of transactions a day.<br />
2. ESMEManager support mutiple binds of different types. Will round-robin on Transmitter and Transceiver bind.<br />
3. Will log the Full PDU to stdout.<br />
4. Support for SQL Server to store PDU's.<br />
5. Reconnection support when connection drop. Implements the Enquire link rules.<br />
6. Supports Encoding for ASCII, Latin1 and UTF-16. Handles Default data coding rules.<br />
7. Event driven API so all the details are taken care of.<br />
8. Working console based test application.<br />

Extended Support<br />
Ardan Studios has frameworks for using the SMPP client in a windows service. We also have bare bones services for an ESME and SMSC implementation.

Contact Ardan Studios at bill@ardanstudios.com for more information.
