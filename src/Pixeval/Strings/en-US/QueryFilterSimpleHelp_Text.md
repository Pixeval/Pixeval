Simplified tutorial gives several examples for most common query cases.

## Simple queries

| Syntax                                    | Meaning                                                                                         |
| ----------------------------------------- | ----------------------------------------------------------------------------------------------- |
| abc                                       | 查询标题包含关键词abc                                                                                    |
| #abc                                      | Any tag that includes 'abc'                                                                     |
| @author name or ID           | Lookup artworks of an author                                                                    |
| -r18g                                     | Exclude r18g artworks                                                                           |
| +gif                                      | Include GIF animations (all constraints: r18, r18g, gif, ai) |
| i:10-                     | Lookup from the tenth illustration of the current page                                          |
| l:100-200                 | 查询大于等于100个，小于等于200个收藏数的                                                                         |
| s:MM-dd                   | Lookup artworks that are published after day dd of month MM of this year                        |
| e:yyyy-MM-dd              | Lookup artworks that are published after day dd of month MM of year yyyy                        |
| r:1.2-3/2 | Lookup illustrations of ratio between 1.2 and 3/2 inclusive, and ignores novels |

## Combinations

| Syntax                            | Meaning                 |
| --------------------------------- | ----------------------- |
| abc #def                          | 标题包含关键词abc 且 标签包含关键词def |
| !#def                             | 标签不包含关键词def             |
| "ab c"                            | 包含空格的关键词查询（如ab c）       |
| abc$ "ab c$"                      | 关键词abc和ab c的准确查询        |
| (or abc$ #def) | 标题是关键词abc 或 标签包含关键词def  |
