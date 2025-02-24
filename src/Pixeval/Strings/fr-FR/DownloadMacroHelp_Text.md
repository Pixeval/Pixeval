Pixeval propose des Path Macros pour affiner les chemins de téléchargement.
Chaque Path Macro est sous forme de `@{name}`. S'il est avec des paramètres, il doit ressembler à `@{name=...}`.\
每个路径宏的形式均为 `@{name}`，有些有参数的宏则是 `@{name=...}`。

Lors qu'un téléchargement démarre, ces macros vont être remplacés par des texts correspondants. Par exemple, `@{id}` sera remplacé par l'ID d'illustration à télécharger.

Si un macro est avec un paramètre, alors ce macro est un macro conditionnnel. Ce macro sera remplacé par son argument si sa condition s'évalue à vraie.
Par exemple, lors d'un téléchargement de manga, `@{if_pic_set=\MANGA\}` sera remplacé par `\MANGA\`.\
例如如果正在下载一副漫画作品，则 `@{if_pic_set=\漫画\}` 会被替换为 `\漫画\`。

Par ailleurs, les macros conditionnels peuvent être imbriqués, comme par exemple: `@{if_pic_set=...@{if_r18=...}...}` .
Des négations peuvent également être utilisées comme `@{!if_pic_set=...}` pour enlever les mangas.\
条件宏也可以使用反模式，例如 `@{!if_pic_set=...}` 表示不是漫画。

Certains macros ont pourtant des contraints à respecter:

- `@{pic_set_index}` doit uniquement être utilsé dans les macros de type `@{if_pic_set=...}`.
- `@{pic_set_index}` et `@{ext}` doivent être utilisés dans les noms des fichiers.

Placer votre souris sur un bouton pour voir l'usage de macro correspondant.
