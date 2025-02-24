Ce manuel complet s'agit principalement des descriptifs des syntaxes, en donnant quelques exemples pour illustrer
tous les types de recherches.

## Syntaxe des recherches

| Syntaxe                                                                            | Signification                                     |
| ---------------------------------------------------------------------------------- | ------------------------------------------------- |
| [str]                          | Titre                                             |
| #[str]                         | Tag                                               |
| @[str \\| num]   | Auteur                                            |
| +[const]                       | 正约束                                               |
| -[const]                       | 反约束                                               |
| i:[int-range]  | 筛选作品的序号                                           |
| l:[int-range]  | Filtrer les artworks par leurs nombres de favoris |
| r:[frac-range] | 筛选插画的横纵比，对小说无效                                    |
| s:[date]       | Date de publication à partir de                   |
| e:[date]       | Date de publication jusqu'à                       |

## Syntaxe des valeurs

### [str] Chaînes de caractères

| Syntaxe | Signification                                         |
| ------- | ----------------------------------------------------- |
| abc     | Chaînes de caractères normales                        |
| "ab# c" | Avec espaces ou caractères spéciales                  |
| abc$    | 完全匹配的字符串                                              |
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

| Syntaxe                                                   | Signification           |
| --------------------------------------------------------- | ----------------------- |
| 2-                                                        | Plus grand ou égale à 2 |
| -3                                                        | Plus petit ou égale à 3 |
| 2-3                                                       | 大于等于2且小于等于3             |
| [2,3] | 数学集合，大于等于2且小于等于3        |
| \[2,3)                         | 数学集合，大于等于2且小于3          |

> 数学集合不支持类似"2-"、"-3"的半开无穷集

### [frac-range] 正实数范围

| Syntaxe                 | Signification             |
| ----------------------- | ------------------------- |
| 2-                      | Plus grand ou égale à 2   |
| -1.5    | Plus petit ou égale à 1,5 |
| -1/2                    | Moins de 1/2 ou égale     |
| 1/2-3                   | 大于等于1/2且小于等于3             |
| 0.3-1/2 | Entre 0,3 et 1/2          |

### [date] Date

| Syntaxe                                    | Signification |
| ------------------------------------------ | ------------- |
| MM-dd                                      | 今年某月某日        |
| MM.dd                      | 今年某月某日        |
| yyyy-MM-dd                                 | 某年某月某日        |
| yyyy.MM.dd | 某年某月某日        |

> Les '.' et '-' peuvent être mélangés.

## 序列

| Syntaxe                                          | Signification |
| ------------------------------------------------ | ------------- |
| !\<segment>                                     | 反模式           |
| (and \<segment> \<segment>) | 和模式           |
| (or \<segment> \<segment>)  | 或模式           |

> 三种模式可以任意嵌套，顶层序列默认使用和模式
