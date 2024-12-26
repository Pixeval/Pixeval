La version simplifiée de manuel donne quelques exemples de recherche.

## Recherche simple

| Syntaxe             | Signification                                                                                  |
|---------------------|------------------------------------------------------------------------------------------------|
| abc                 | Recherche sur les titres avec mot clé 'abc'                                                    |
| #abc                | Recherche sur le tag 'abc'                                                                     |
| @nom d'auteur ou ID | Recherche des artworks d'un auteur                                                             |
| -r18g               | Exclure les artworks r18g                                                                      |
| +gif                | Inclure les animations GIF (contraints: r18, r18g, gif, ai)                                    |
| i:10-               | Afficher les illustrations à partir de la dixième de page actuelle                             |
| l:100-200           | Recherche sur les illustrations avec nombre de favoris entre 100 et 200 (compris)              |
| s:MM-dd             | Recherche des artworks publiés après jour dd du mois MM de l'année courante                    |
| e:yyyy-MM-dd        | Recherche des artworks publiés après jour dd du mois MM de l'année yyyy                        |
| r:1.2-3/2           | Recherche des illustrations avec un ratio entre 1,2 et 3/2 compris, non valide pour les romans |

## Recherche combinatoire

| Syntaxe        | Signification                                          |
|----------------|--------------------------------------------------------|
| abc #def       | Titre contient mot clé 'abc' **et** avec tag 'def'     |
| !#def          | Exclure tag 'def'                                      |
| "ab c"         | Titre contient mot clé 'ab c' avec un **espace**       |
| abc$ "ab c$"   | Recherche **exacte** des mots clés 'abc' **et** 'ab c' |
| (or abc$ #def) | Recherche par un mot clé 'abc' **ou** un tag 'def'     |
