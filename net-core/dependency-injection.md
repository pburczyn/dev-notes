## Lifetimes - cykl życia instancji

Lifetimes na przykładzie <U>GuidGenerator'a</U>

| Lifetimes | Request #1<br/>Controller | Request #1<br/>Service |  Request #2<br/>Controller | Request #2<br/>Service |
|--|--|--|--|--|
|Transient|69784ae2..|9db240f8...|de2642b4...|d7915ed6...|
|Scoped|92fee789...|92fee789...|c1032331...|c1032331...|
|Singleton|019a46a1...|019a46a1...|019a46a1...|019a46a1...|

**Transient** - każde żądanie nowa instancja
**Scoped** - instancja per request, dla jednego przetwarzanego request zostanie utworzona jedna instancja
**Singleton** - jeden instancja na cały cykl życia aplikacji
