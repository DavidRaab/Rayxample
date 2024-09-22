This folder contains three different implementations of an particle system.

1. One uses just normal records and swaping of those elements.
2. The _struct prefix replaced it with a Struct. This changes the API
   and array indices must be passed.
3. The _struct_array prefix uses two array of structs. I thought it could be
   better to have two array where results of processing are put into another
   array. But turned out to be the worsest solution.
   