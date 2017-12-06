# Console Instructions

All actions of the clients and the server and any incoming or outgoing transmissions are displayed in the console. New entries are appended to the bottom and the console scrolls automatically, as long as the scroll bar was already at the bottom.

The first column carries a timestamp with the ID of the respective client or server and it shows a description of the action performed. If data was transferred, it will be displayed in hexadecimal representation in the 2nd and as raw text in the 3rd column.

>### Note
>
>You can hide the timestamp by unchecking *Add Timestamp* under *Console*.
>
>You can hide the hexadecimal representation of the data by unchecking *Translate Hex* under *Console*. This also displays the raw text in its original format.
>
>Control characters contained in the raw text will be replaced by the configured replacer character. This won’t replace whitespace, unless *Translate Hex* is enabled.

## Clearing the Console

Right-click on the console and choose *Clear Console* from the context menu.

## Copying Console Content as Log

Right-click on the console and choose *Copy Console* from the context menu. This will copy all the current content from the console to the clipboard in a line-by-line format, so you can paste it into any text file.
