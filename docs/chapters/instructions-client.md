# Client Instructions

## Sending Requests

In *IP* under *Remote Host* enter the IP address or the FQDN of the server and in *Port* enter the destination port.

Feed the data you want to send into the request field.

### Manually

Click on *Send Request*.

### Automatically

Check the field *Automatic*, configure a time interval in *Interval* (measured in milliseconds) and click on *Start Timer*.

>>#### :information_source:
>
>The request field is read-only, as long as the field *Automatic* is checked. This also applies to the fields *IP* and *Port* under *Remote Host*.
>
>With each click on *Send Request* and each tick of the timer, a new TCP client is created, which transfers the data to the server and receives the response, if need be. These independent clients can be identified in the console by their unique numeric ID.
