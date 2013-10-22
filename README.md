ParseCityCouncilToDB
====================
This is a light weight proof of concept that breaks apart an Omaha City Council Journal (or agenda) into manageable, searchable, bits.  Each section is broken into its own row and header, as are 'pursuant' rules into their own section.  

Future iterations may include breaking sections down to who voted for them, picking apart business names/info/etc, but right now it is enough to have journals in a more database friendly format.

I also included an example of the current iteration database schema.. it follows four tables.. the main journal table, the child table with the sections's info, header, and journal id, the entire file's backup (in a table to speed up db access), and the pursuant to chunks.

At the time this is an effective plan for getting the data into chunks.

The code to do this is just a series of for loops, if statements, and a custom class.. nothing that can't be recoded in another language.


C# 4.0
