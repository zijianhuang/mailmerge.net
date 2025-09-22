BulkMailMerge is a CLI app that sends mass Email messages to recipients one by one. And the Email message body and subject are optionally merged with CSV or JSON data if the body and the subject contain placeholders of [Handlebars](https://handlebarsjs.com/).

**Prerequisites**
1. .NET 9 (Desktop) Runtime installed.
1. SMTP Config is well prepared in appsettings.json 

When running BulkMailMerge.exe without parameter, you see basic parameters and examples.

```
 ./BulkMailMerge.exe
Bulk send Email messages merged with CSV or JSON data to contacts.
BulkMailMerge  version 1.1.0.0


   /ContactList, /CL   Array of Email addresses, e.g., /CL=some@where.com hello@kitty.net . If CF is also not
                       declared, Email addresses in data file will be used.
   /ContactFile, /CF   List of Email addresses line by line, e.g., /CF=EmailAddresses.txt . Optional if mail merge is
                       utilized. If CL is also not declared, Email addresses in data file will be used.
   /SubjectFile, /SF   Email subject in file, e.g., /SF=subject.txt.
   /BodyFile, /BF      Email body or template in text file, e.g., /BF=body.html . If the file ext is html or htm, the
                       Email will be in HTML format, otherwise, plain text.
   /DataFile, /DF      CSV or array of JSON data in text file to merge with the body template, e.g., /DF=data.json or
                       /DF=data.csv. The data file must have a field for Email address.
   /KeyField, /KF      For mail merge with data, the key field to match the Email address in the contact file, e.g.,
                       /KF=EmailAddress . Optional. If not defined, the first field is used.
   /InReplyToPrefix,   For recipient to group or filter messages. Default: EmailQueue. Each Email sent has
   /IRTP               Prefix+GUID as InReplyTo, e.g., /IRTP=MyCompany, then InReplyTo will become MyCompany_GUID for
                       each Email message sent.
   /Help, /h, /?       Shows this help text



Examples:
BulkMailMerge.exe /CL=Hello@Kitty.net ~ The recepient will receive a test Email if the appsettings.json has correct SMTP config.
BulkMailMerge.exe /CF=ContactList.txt /SF=Subject.txt /BF=Body.txt ~ The recipients in ContactList.txt will receive an Email message based on the subject and the body.
BulkMailMerge.exe /CF=Profiles/ProductUpdate/ContactList.txt /SF=Profiles/ProductUpdate/Subject.txt /BF=Profiles/ProductUpdate/Body.html ~ The recipients in ContactList.txt will receive an Email message based on the subject and the HTML body.
BulkMailMerge.exe /CF=Profiles/ProductUpdate/ContactList.txt /SF=Profiles/ProductUpdate/Subject.txt /BF=Profiles/ProductUpdate/Body.html /DF=data.json  ~ What in data.json will be merged with the body template, and the first field of JSON objects is expected to be Email address.
../../App/BulkMailMerge.exe /SF=Subject.txt /BF=Body.html /DF=data.csv /KF=mailbox ~ the mailbox field of the CSV will be used as recipient.
```
### appsettings.json

```json
{
    "mailSettings": {
        "smtp": {
            "host": "email2-smtp.us-east-2.amazonaws.com",
            "port": 2587,
            "userName": "AKIATWCTV84Y5ZYUEPAY",
            "password": "BGh1vJKbsq9ot3KOHvpLRjYOujsMUeeJ8BBcV/10T889",
            "from": "noreply@mydomain.net",
            "enableSsl": true,
            "enableTls": true
        }
    }

}
```



## Scenarios

### Verify If SMTP config works

```
BulkMailMerge.exe /CL=Your@EmailAddress.come 
```

If your mailbox of Your@EmailAddress.come receive Email message "Test Subject", you know the config is working well with the SMTP server you have setup.

### Send Email messages to all contacts with the same subject and the same message body

```
BulkMailMerge.exe /CF=ContactList.txt /SF=Subject.txt /BF=Body.txt
```

or 

```
BulkMailMerge.exe /CF=ContactList.txt /SF=Subject.txt /BF=Body.html
```

### Send Email messages merging with recipient specific data

```
BulkMailMerge.exe /CF=Profiles/ProductUpdate/ContactList.txt /SF=Profiles/ProductUpdate/Subject.txt /BF=Profiles/ProductUpdate/Body.html
```
The recipients in ContactList.txt will receive an Email message based on the subject and the HTML body.

```
BulkMailMerge.exe /CF=Profiles/ProductUpdate/ContactList.txt /SF=Profiles/ProductUpdate/Subject.txt /BF=Profiles/ProductUpdate/Body.html /DF=data.json
```
What in data.json will be merged with the body template, and the first field of JSON objects is expected to be Email address.

```
../../App/BulkMailMerge.exe /SF=Subject.txt /BF=Body.html /DF=data.csv /KF=mailbox
```
The mailbox field of the CSV will be used as recipient.

**Hints**
* When Mail Merge is to be executed, it is handy to have one test recipient so you have have a test drive to ensure the merged content is OK before sending mass messages.

For example, do this first:
```
../../App/BulkMailMerge.exe /CL=mytest@email.com /SF=Subject.txt /BF=Body.html /DF=data.csv /KF=mailbox
```
mytest@email.com should be included in data.csv. So even data.csv has 1000 rows with 1000 recipients, there will be only one message being sent out.

Then run:
```
../../App/BulkMailMerge.exe /SF=Subject.txt /BF=Body.html /DF=data.csv /KF=mailbox
```

### Hints

If the HTML body should has media like images, the images had better be presented as embedded resources typically through BASE64 encoding.

If the same image like icon is presented multiple times, the embedded resources could be stored in the embedded stylesheet.

## Limitations of the App

This is a lightweight mass Email application, not like other commercial mass Email services which provide rich "value added" services that you may or may not need. There is no comprehensive monitoring, recovery and analytical features etc.

Limitation:
1. With Email servers like Amazon Simple Email Service (SES), the speed is around 1 mail per second.
2. 

### Hints
* Before sending an Email message, the app will check if the connection with the Email server is connected and authenticated. If not, the app will reestablish the authenticated connection that may take a few seconds.
* When sending an Email message if there's a recoverable error, the app may try to send it only once again.
* Depending on the error types during sending an Email message, the app may skip and continue with next recipients, or exit. 
* The logging messages are written into the console only.
* The app can run without user logging, and this is handy for running the app with Windows Scheduler and alike.