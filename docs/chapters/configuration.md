# Configuration

Click on the menu *Settings* and choose *Preferences…* to open the preferences window.

>>#### :information_source:
>
>The settings are not persisted, they are lost upon exiting the application.

## Connection

### Send Timeout

The time in milliseconds, the clients and the server wait for confirmation after attempting a connection or sending data. If this time is exceeded, the connection is considered faulty and is terminated.

### Receive Timeout

The time in milliseconds, the clients and the server wait for a response after sending data. If this time is exceeded, the connection is considered faulty and is terminated.

## Text Processing

### Replacer for Control Characters

A single character, used by the console to replace non-printable characters.

### Hexadecimal Output Format

A string composed of `{0}` and two freely chosen characters (with the exception of `{` and `}`). It determines the format of the hexadecimal representation in the console. `{0}` stands in place of the hexadecimal characters.

### Translate Hexadecimal Characters in Request and Response

If this field is checked, data to be transferred will be searched for characters in hexadecimal representation. If any are found, they will be replaced with the respective character before  transmission (e. g.: `0x42` is replaced by `B`). That way, hexadecimal values with 2, 4, 6 or 8 characters or 1-4 Bytes can be entered.

### Hexadecimal Input Format

A string composed of `{0}` and two freely chosen characters (with the exception of `{` and `}`). It determines the format of the hexadecimal representation in the request and response field, as well as in Commands. `{0}` stands in place of the hexadecimal characters.

## Encoding

### Client

The encoding, which is used by the clients. It determines how raw text is converted to bytes before transmission and how received bytes are converted to raw text before it’s displayed.

### Server

The encoding, which is used by the server. It determines how raw text is converted to bytes before transmission and how received bytes are converted to raw text before it’s displayed.
