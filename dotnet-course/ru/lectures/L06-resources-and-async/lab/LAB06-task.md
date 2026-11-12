# Лабораторная 6. Журнал заказов: потоковый разбор большого файла

**К лекции:** [Л6. C#: ресурсы, файлы и асинхронность](../L06-resources-and-async.md)
**Условие:** всё — из терминала, как и раньше. **Сдаётся вживую:** отчёта нет, вы показываете
результат и отвечаете на вопрос под каждым шагом.

## Что должно быть готово

У `HelloWorld` из прошлых лабораторных появился режим `digest`: он читает журнал заказов на пять
миллионов строк **построчно и асинхронно**, считает заказы по статусам, пишет итоги в файл через
**`await using`**, останавливается по **timeout** и в конце сам проверяет, что оба файла
свободны. Нужны `bash` и `awk` (на Windows — Git Bash или WSL) и около полугигабайта на диске.

```text
$ dotnet run -- digest orders.csv totals.txt
peak memory: 49 MB
orders.csv: free
totals.txt: free
$ dotnet run -- digest orders.csv totals.txt 1
cancelled after 1 s
peak memory: 55 MB
orders.csv: free
totals.txt: free
```

## 1. Файл

Положите в папку `HelloWorld` файл `gen-orders.sh` и запустите его оттуда:

```sh
#!/usr/bin/env bash
awk 'BEGIN {
    split("Created Paid Shipped Delivered Cancelled", status, " ")
    for (i = 1; i <= 5000000; i++) {
        printf "%d,customer%d@example.com,%s,%d.%02d\n", i, i % 1000, status[i % 5 + 1], i % 1000, i % 100
    }
}'
```

Затем `chmod +x gen-orders.sh`, `./gen-orders.sh > orders.csv` — около 223 мегабайт, строка
`номер,почта,статус,сумма` — и добавьте в `.gitignore` строки `*.csv` и `totals.txt`.

**Вопрос:** относительно чего программа ищет `orders.csv`, когда вы запускаете `dotnet run` из
папки `HelloWorld`?

## 2. Потоковый разбор

Напишите в `HelloWorld` класс `OrderLog` с методом
`DigestAsync(string input, string output, CancellationToken ct)`:

1. Reader — `using StreamReader` на `input`, Writer — `await using StreamWriter` на `output`.
2. Цикл `ReadLineAsync(ct)`, пока не вернётся `null`, как на кадре 52 лекции. Статус — третье
   поле строки: разберите его `Enum.TryParse` в `OrderStatus` и прибавьте единицу в словаре по
   статусу.
3. После цикла запишите итоги, по строке на статус, через `await writer.WriteLineAsync(...)`.

В `Program.cs` при аргументах `digest <журнал> <итоги> [секунды]` вызовите `DigestAsync`, а после
него напечатайте пиковую память — `Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024`
из `System.Diagnostics`. Без аргументов `HelloWorld` запускает оболочку, как раньше.

**Вопрос:** `File.ReadAllTextAsync` на этом файле поднимает пиковую память почти до гигабайта.
Что лежит в памяти у вашей версии и почему она не вырастет на файле в десять раз больше?

## 3. Timeout и «файл свободен»

1. В `Program.cs` создайте cancellation token source и передайте его token в `DigestAsync`, а
   внутри — в каждый `ReadLineAsync`:

   ```csharp
   using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
   ```

2. Поймайте `OperationCanceledException` один раз — вокруг вызова `DigestAsync`, — напечатайте
   `cancelled after N s` и верните код `2` (проверьте `echo $?`).
3. После разбора, удачного или отменённого, напечатайте `FileCheck.IsFree` для обоих файлов:

   ```csharp
   public static class FileCheck
   {
       public static string IsFree(string path)
       {
           try
           {
               using FileStream probe = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
               return $"{path}: free";
           }
           catch (IOException ex)
           {
               return $"{path}: BUSY ({ex.Message})";
           }
       }
   }
   ```

**Вопрос:** отмена бросила исключение посреди цикла. Кто закрыл Reader и Writer, хотя
`DigestAsync` не дошёл до конца?

**Вопрос (усложнённый):** почему проверять «файл свободен» нужно изнутри процесса, а не другой
программой после его завершения?

Соберите оба проекта без предупреждений и закоммитьте; `*.csv` и `totals.txt` в git не попадают.

## Дополнительное задание (по желанию)

Напишите `async IAsyncEnumerable<string> ReadLinesAsync(string path, [EnumeratorCancellation]
CancellationToken ct = default)` с `using StreamReader` и `yield return` внутри, как на кадре 53,
и замените цикл чтения в `DigestAsync` на `await foreach`. Где теперь закрывается Reader?

## Как проходит приёмка

Принимающий просит запустить `digest` целиком и с timeout в одну секунду и может попросить
убрать `await using` у Writer — сначала предсказать, потом показать и объяснить, что изменилось.
