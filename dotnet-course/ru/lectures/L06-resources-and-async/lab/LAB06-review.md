# Лабораторная 6 — заметки принимающему

Служебный файл. Студенту выдаётся [LAB06-task.md](LAB06-task.md); здесь — что имеет смысл
посмотреть и о чём спросить. Это рекомендации, а не регламент.

**Приёмка живая, отчёта нет**, минут на десять. Студент за своим терминалом, журнал
`orders.csv` сгенерирован. Три главные вещи:

1. Файл читается **построчно и асинхронно**: `StreamReader` + `ReadLineAsync(ct)`, без
   `ReadAllText`. Пиковая память — десятки мегабайт, а не гигабайт.
2. **Владение ресурсом**: Reader — `using`, Writer — `await using`, cancellation token source —
   `using`. После работы, удачной и отменённой, `IsFree` печатает `free` для обоих файлов.
3. **Отмена**: token протянут до каждого `ReadLineAsync`, `OperationCanceledException` пойман
   один раз в `Program.cs`, код выхода `2`.

## На что смотреть

| Что попросить показать | На что обратить внимание |
|---|---|
| `DigestAsync` | `using StreamReader`, `await using StreamWriter`; цикл `ReadLineAsync(ct)` до `null`; словарь по статусу, а не список строк |
| `Program.cs` | `using CancellationTokenSource`; один `catch (OperationCanceledException)` вокруг вызова, код `2`; `IsFree` на обоих путях |
| `cat totals.txt`, `git status` | Пять строк по 1 000 000 заказов; `orders.csv` и `totals.txt` не в индексе |

**Эталонное решение** (dotnet SDK 10.0.114) печатает ровно сессию из задания, а `totals.txt` —

```text
Paid: 1000000 orders
Shipped: 1000000 orders
Delivered: 1000000 orders
Cancelled: 1000000 orders
Created: 1000000 orders
```

Генератор детерминирован: по миллиону на статус у всех. Порядок строк — порядок словаря, он
может быть другим. Полный разбор занимает около полутора секунд, поэтому timeout в одну
секунду срабатывает всегда; после отмены `totals.txt` пуст — итоги пишутся после цикла.

**Правка на приёмке: Writer без `await using`.** Попросите предсказать до запуска. Эталонное
решение печатает:

```text
peak memory: 48 MB
orders.csv: free
totals.txt: BUSY (The process cannot access the file '/home/user/HelloWorld/totals.txt' because it is being used by another process.)
```

и `totals.txt` после выхода — ноль байт: пять строк остались в buffer Writer, который никто не
сбросил. «Другой процесс» — собственный незакрытый Writer, ровно кадр 11 лекции.

## Что говорит компилятор

| Правка | Результат |
|---|---|
| Убрать `await` у `ReadLineAsync` | `error CS0029: Cannot implicitly convert type 'System.Threading.Tasks.ValueTask<string?>' to 'string'` |
| `await using` у `StreamReader` | `error CS8417: 'StreamReader': type used in an asynchronous using statement must implement 'System.IAsyncDisposable' or implement a suitable 'DisposeAsync' method. Did you mean 'using' rather than 'await using'?` |
| Убрать `await` у `WriteLineAsync` | `warning CS4014: Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.` |
| Token не передан в `ReadLineAsync` | Ничего. С timeout в секунду разбор доходит до конца, код выхода `0` |

Последняя строка — главное: самую дорогую ошибку лекции компилятор не видит, её выдаёт только
timeout.

## Вопросы и короткие ответы

- Относительно чего ищется `orders.csv`?
  *Относительно рабочей директории процесса, а не папки сборки.*
- Что в памяти у потоковой версии?
  *Buffer Reader на несколько килобайт, одна строка и словарь на пять статусов. Строка в .NET —
  два байта на символ, поэтому файл в 223 МБ целиком — строка около 450 МБ плюс промежуточные
  buffer: на эталонном решении `ReadAllTextAsync` дал 995 МБ против 49.*
- Кто закрыл Reader и Writer при отмене?
  *`using` и `await using` развернулись в `finally`: исключение отмены летит вверх к
  `Program.cs`, и по дороге Writer сбрасывает buffer и закрывается, Reader закрывается.*
- Почему проверка изнутри процесса (усложнённый)?
  *После выхода ОС закрывает все дескрипторы сама — снаружи утечку не увидеть. Сервис живёт
  неделями, и «закроется при выходе» для него значит «никогда».*
- Где закрывается Reader в `ReadLinesAsync` (доп.)?
  *В `finally` итератора: `await foreach` вызывает `DisposeAsync` у перечислителя, в том числе
  при раннем выходе. Задание опирается на `yield return` с кадра 53, в Л5 он не преподавался —
  не требуйте больше, чем дала лекция.*

## Возможные ошибки

**Файл читается целиком** (`ReadAllText`, `ReadToEndAsync`). Попросите показать пиковую память.

**Token принят, но не передан** или `ReadLine` вместо `ReadLineAsync`. Компилятор молчит;
попросите запустить с timeout в секунду — разбор дойдёт до конца.

**`catch (OperationCanceledException)` внутри `DigestAsync`.** Неполные итоги пишутся как
полные, код `0`. Спросите, как отличить отменённый разбор от законченного.

**`IsFree` только на удачном пути.** Главный случай — отменённый.
