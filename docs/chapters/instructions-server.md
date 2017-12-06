# Server Instructions

## Hosting a Server

In *Port* under *Local Host* enter the port, under which the server should be available and then click on *Start Server*.

>#### :information_source: Notes
>
>The current server status is displayed in the lower left corner of the main window.
>
>The field *Port* under *Local Host* is read-only, as long as the server is running.

## Responding to Requests

### Manually

Feed the data to be transferred into the response field and click on *Send Response*, when a client is connected.

### Automatically

Feed the data to be transferred into the response field and check the field *Automatic*.

Now the server will respond to every request with the given data.

>#### :information_source: Notes
>
>The response field is read-only, as long as the field *Automatic* is checked.

### Automatically using Commands

Check the fields *Automatic* and *Use Commands*.

If the server receives a request now, it will search the column *Request* for matching data and if there is a matching entry, it will respond with the data from the column *Response*.

To create a Command, double-click on the last row, fill in *Request* and then *Response* and confirm with the enter key.

To delete a Command, click on the respective row and press the delete key on your keyboard.

>#### :information_source: Notes
>
>The request part of a Command can’t be empty.

### Subsequent Socket Shutdown

Check the field *Shutdown Socket*.

After sending a response, the server will now disable sending on the connected socket. This will signal the connected client to disconnect. This also applies, if the transferred response was empty.
