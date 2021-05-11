## Phonebook

### Description

You need to create an executable that would manage a phone book. The commands you need to support are:

- phone-book.exe /path/to/file add [name] [phone]
- phone-book.exe /path/to/file list [skip], [limit]

The output of the list operation must be the phone book records in lexical order. You may not sort the data during the list operation, however. All such work must be done in the add operation.

You may keep any state you’ll like in the file system, but there are separate invocations of the program for each step.  This program need to support adding 10 million records.

Feel free to constrain the problem in any other way that would make it easier for you to implement it.

### Notes

1. I don't need to write a parser for commandline, most of the thought went into adding and processing the solution