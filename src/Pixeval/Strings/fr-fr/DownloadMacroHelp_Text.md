Pixeval propose des Path Macros pour affiner les chemins de téléchargement.
Chaque Path Macro est sous forme de '@{name}'. S'il est avec des paramètres, il doit ressembler à '@{name=...}'.

Lors qu'un téléchargement démarre, ces macros vont être remplacés par des texts correspondants. Par exemple, '@{id}' sera remplacé par l'ID d'illustration à télécharger.

Si un macro est avec un paramètre, alors ce macro est un macro conditionnnel. Ce macro sera remplacé par son argument si sa condition s'évalue à vraie.
Par exemple, lors d'un téléchargement de manga, '@{if_manga=\MANGA\}' sera remplacé par '\MANGA\'.

Placer votre souris sur un bouton pour voir l'usage de macro correspondant.
