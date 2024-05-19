Pixeval propose des Path Macros pour affiner les chemins de téléchargement.
Chaque Path Macro est sous forme de `@{name}`. S'il est avec des paramètres, il doit ressembler à `@{name=...}`.

Lors qu'un téléchargement démarre, ces macros vont être remplacés par des texts correspondants. Par exemple, `@{id}` sera remplacé par l'ID d'illustration à télécharger.

Si un macro est avec un paramètre, alors ce macro est un macro conditionnnel. Ce macro sera remplacé par son argument si sa condition s'évalue à vraie.
Par exemple, lors d'un téléchargement de manga, `@{if_manga=\MANGA\}` sera remplacé par `\MANGA\`.

Par ailleurs, les macros conditionnels peuvent être imbriqués, comme par exemple: `@{if_manga=...@{if_r18=...}...}` .
Des négations peuvent également être utilisées comme `@{!if_manga=...}` pour enlever les mangas.

Certains macros ont pourtant des contraints à respecter:
- `@{manga_index}` doit uniquement être utilsé dans les macros de type `@{if_manga=...}`.
- `@{manga_index}` et `@{ext}` doivent être utilisés dans les noms des fichiers.

Placer votre souris sur un bouton pour voir l'usage de macro correspondant.
