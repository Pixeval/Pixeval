## Simple queries

| Syntax             | Meaning                                                                         |
|--------------------|---------------------------------------------------------------------------------|
| abc                | Any title that includes 'abc'                                                   |
| #abc               | Any tag that includes 'abc'                                                     |
| @author name or ID | Lookup artworks of an author                                                    |
| -r18g              | Exclude r18g artworks                                                           |
| +gif               | Include GIF animations (all constraints: r18, r18g, gif, ai)                    |
| i:10-              | Lookup from the tenth illustration of the current page                          |
| l:100-200          | Lookup artworks that have numbers of likes between 100 and 200 (both inclusive) |
| s:MM-dd            | Lookup artworks that are published after day dd of month MM of this year        |
| e:yyyy-MM-dd       | Lookup artworks that are published after day dd of month MM of year yyyy        |

## Combinations

| Syntax         | Meaning                                                                 |
|----------------|-------------------------------------------------------------------------|
| abc #def       | Artworks with a title containing 'abc' and a tag 'def'                  |
| !#def          | Artworks that **do not** contain tag 'def'                              |
| "ab c"         | Artworks with a title containing 'ab c' which has a **space**           |
| abc$ "ab c$"   | Artworks with **exact** match of keywords 'abc' **and** 'ab c'          |
| (or abc$ #def) | Artworks with a title that's an exact match of 'abc' **or** a tag 'def' |
