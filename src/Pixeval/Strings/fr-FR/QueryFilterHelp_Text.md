Ce manuel complet s'agit principalement des descriptifs des syntaxes, en donnant quelques exemples pour illustrer
tous les types de recherches.

## Syntaxe des recherches

| Syntaxe                                                                            | Signification                                                     |
| ---------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| [str]                          | Titre                                                             |
| #[str]                         | Tag                                                               |
| @[str \\| num]   | Auteur                                                            |
| +[const]                       | Contraints positifs                                               |
| -[const]                       | Contraints négatifs                                               |
| i:[int-range]  | Filtrer les artworks par leurs indices                            |
| l:[int-range]  | Filtrer les artworks par leurs nombres de favoris                 |
| r:[frac-range] | Filtrer les artworks selon leurs ratios, invalide pour les novels |
| s:[date]       | Date de publication à partir de                                   |
| e:[date]       | Date de publication jusqu'à                                       |

## Syntaxe des valeurs

### [str] Chaînes de caractères

| Syntaxe | Signification                                         |
| ------- | ----------------------------------------------------- |
| abc     | Chaînes de caractères normales                        |
| "ab# c" | Avec espaces ou caractères spéciales                  |
| abc$    | Un exact match                                        |
| "ab c$" | Un exact match avec espace et/ou caractères spéciales |

### [num] Nombres

| Syntaxe | Signification   |
| ------- | --------------- |
| 12345   | Nombres normaux |

### [const] Contraints

| Syntaxe | Signification                 |
| ------- | ----------------------------- |
| r18     | Contenu de R18 y compris R18G |
| r18g    | Contenu de R18G               |
| gif     | Animation GIF                 |
| ai      | Contenu généré par l'IA       |

### [int-range] Intervalles des intègres

| Syntaxe                                                   | Signification                                                                         |
| --------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| 2-                                                        | Plus grand ou égale à 2                                                               |
| -3                                                        | Plus petit ou égale à 3                                                               |
| 2-3                                                       | Entre 2 et 3 inclus                                                                   |
| [2,3] | Format intervalle, entre 2 et 3 inclus                                                |
| \[2,3)                         | Format intervalle, entre 2 (inclu) et 3 (exclu) |

> Les intervalles demi ouvertes comme '2-' ou '-3' ne sont pas supportées par le format d'intervalle.

### [frac-range] Intervalles des décimaux positifs

| Syntaxe                 | Signification             |
| ----------------------- | ------------------------- |
| 2-                      | Plus grand ou égale à 2   |
| -1.5    | Plus petit ou égale à 1,5 |
| -1/2                    | Moins de 1/2 ou égale     |
| 1/2-3                   | Entre 1/2 et 3, compris   |
| 0.3-1/2 | Entre 0,3 et 1/2          |

### [date] Date

| Syntaxe                                    | Signification                      |
| ------------------------------------------ | ---------------------------------- |
| MM-dd                                      | jour dd du mois MM de cette année  |
| MM.dd                      | jour dd du mois MM de cette année  |
| yyyy-MM-dd                                 | jour dd du mois MM de l'année yyyy |
| yyyy.MM.dd | jour dd du mois MM de l'année yyyy |

> Les '.' et '-' peuvent être mélangés.

## Combinatoires

| Syntaxe                                          | Signification                                           |
| ------------------------------------------------ | ------------------------------------------------------- |
| !\<segment>                                     | Négation                                                |
| (and \<segment> \<segment>) | Pattern 'et', tous les segments doivent être satisfaits |
| (or \<segment> \<segment>)  | Pattern 'ou', un segment satisfait suffit               |

> Ces trois patterns peuvent être imbriqués. Les segments de niveau plus haut est en mode 'et' par défaut.
