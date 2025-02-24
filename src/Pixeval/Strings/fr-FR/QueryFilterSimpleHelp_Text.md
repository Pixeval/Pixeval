简易教程以举例形式说明常见的查询方式

## Recherche simple

| Syntaxe                                   | Signification                                                                                  |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------- |
| abc                                       | Recherche sur les titres avec mot clé 'abc'                                                    |
| #abc                                      | Recherche sur le tag 'abc'                                                                     |
| @nom d'auteur ou ID          | Recherche des artworks d'un auteur                                                             |
| -r18g                                     | Exclure les artworks r18g                                                                      |
| +gif                                      | Inclure les animations GIF (contraints: r18, r18g, gif, ai) |
| i:10-                     | Afficher les illustrations à partir de la dixième de page actuelle                             |
| l:100-200                 | 查询大于等于100个，小于等于200个收藏数的                                                                        |
| s:MM-dd                   | Recherche des artworks publiés après jour dd du mois MM de l'année courante                    |
| e:yyyy-MM-dd              | Recherche des artworks publiés après jour dd du mois MM de l'année yyyy                        |
| r:1.2-3/2 | Recherche des illustrations avec un ratio entre 1,2 et 3/2 compris, non valide pour les romans |

## Recherche combinatoire

| Syntaxe                           | Signification                                          |
| --------------------------------- | ------------------------------------------------------ |
| abc #def                          | Titre contient mot clé 'abc' **et** avec tag 'def'     |
| !#def                             | 标签不包含关键词def                                            |
| "ab c"                            | 包含空格的关键词查询（如ab c）                                      |
| abc$ "ab c$"                      | Recherche **exacte** des mots clés 'abc' **et** 'ab c' |
| (or abc$ #def) | 标题是关键词abc 或 标签包含关键词def                                 |
