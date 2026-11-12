# Лабораторная 7. Магазин по HTTP: сервер на socket'ах

**К лекции:** [Л7. Веб-фундамент: HTTP и стили взаимодействия](../L07-web-fundamentals.md)
**Условие:** из терминала, без веб-фреймворков: только `TcpListener`, stream байтов и ваши руки.
Работы — минут на двадцать. **Нужно:** .NET 10 SDK, `curl`, `Shop.Domain` с `Total()` из пятой.
**Сдаётся вживую:** отчёта нет. Вы показываете результат и отвечаете на вопросы.

**Что строим:** консольный `Shop.RawServer` рядом с `Shop.Domain`, порт `8088`. Все response'ы —
`text/plain; charset=utf-8`:

| Request | Response |
|---|---|
| `GET /` | `200`, `Магазин работает` |
| `GET /orders` | `200`, по строке на заказ, как `list` в оболочке: `1  Ann Smith  Paid  300.00` |
| `GET /orders/<номер>` | `200` и один заказ; нет такого — `404`, `no order 7` |
| другой метод на этих адресах | `405` и header `Allow: GET` |
| любой другой адрес | `404` |

## 1. Проект: заказы и listener

`dotnet new console --name Shop.RawServer`, в его папке — `dotnet add reference ../Shop.Domain`.
`OrderStore.cs` — заказы в памяти, номера с единицы, про HTTP не знает. Если методы вашего
`Order` называются иначе, поправьте три строки:

```csharp
using Shop.Domain;

public sealed class OrderStore
{
    private readonly List<Order> orders = new List<Order>();
    public OrderStore()
    {
        Order ann = new Order(new Customer(new Email("ann@example.com"), "Ann Smith"));
        ann.Add(new OrderItem("Green tea", 2, new Money(150.00m)));
        ann.MoveTo(OrderStatus.Paid);
        Order bob = new Order(new Customer(new Email("bob@example.com"), "Bob Brown"));
        bob.Add(new OrderItem("Coffee", 1, new Money(120.00m)));
        orders.AddRange([ann, bob]);
    }
    public IReadOnlyList<Order> All => orders;
    public Order? Find(int number) => number >= 1 && number <= orders.Count ? orders[number - 1] : null;
}
```

`Program.cs` — listener и цикл приёма с token'ом из Ctrl+C:

```csharp
using System.Net;
using System.Net.Sockets;
using CancellationTokenSource stop = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) => { e.Cancel = true; stop.Cancel(); };
OrderStore store = new OrderStore();
using TcpListener listener = new TcpListener(IPAddress.Loopback, 8088);
listener.Start();
try
{
    while (true)
    {
        using TcpClient client = await listener.AcceptTcpClientAsync(stop.Token);
        await Server.HandleAsync(client.GetStream(), store, stop.Token);
    }
}
catch (OperationCanceledException) when (stop.IsCancellationRequested)
{
    Console.WriteLine("stopped");
}
```

**Вопрос:** заказы живут в памяти процесса. Нарушает ли сервер правило третьего раздела лекции?
Что увидят клиенты после перезапуска и что — если экземпляров два?

## 2. Request → response

Напишите `static class Server` с методом
`static async Task HandleAsync(Stream stream, OrderStore store, CancellationToken ct)`:

1. Читайте stream по байту (`ReadAsync` с `ct`), пока последние четыре не станут `\r\n\r\n`.
   `ReadAsync` вернул 0 — клиент ушёл, выйдите.
2. Байты → строка через `Encoding.ASCII`, разбейте по `"\r\n"`. Первая строка — request line:
   метод, путь, версия через пробел; не три части — `400`. Headers — в словарь:

   ```csharp
   Dictionary<string, string> headers = lines.Skip(1).Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
       .ToDictionary(p => p[0], p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);
   ```

3. Router — одно switch-выражение с паттернами списка, как в четвёртой лабораторной, над
   `(method, path.Split('/', StringSplitOptions.RemoveEmptyEntries))`.
4. Body — `Encoding.UTF8.GetBytes(text)`. В stream: status line (`HTTP/1.1 404 Not Found`),
   `Content-Type`, `Content-Length` — **длина массива байтов**, `Allow: GET` для `405`,
   `Connection: close`, пустая строка — каждая на `\r\n`, — затем body.

**Вопрос:** в `Магазин работает` с переводом строки 17 символов. Какой `Content-Length` отдал
сервер и откуда разница?

## 3. Проверка и одна поломка

1. Пройдите `curl -i` по каждой строке таблицы; для `405` — `curl -i -X DELETE ...`.
2. Прибавьте к `Content-Length` единицу и уберите `using` у `TcpClient`. Запустите
   `curl -i --max-time 3 http://localhost:8088/`. Верните оба.

**Вопрос:** что напечатал `curl` и почему он ждал, хотя body пришёл целиком?

**Вопрос (усложнённый):** лишнюю единицу оставьте, `using` верните. Что напечатал `curl` теперь и
что видно про длину и про закрытие connection'а по отдельности?

Сборка без предупреждений; проект под git: `git init`, `dotnet new gitignore`, commit.

## Дополнительное задание (по желанию)

Keep-alive: request'ы в одном connection'е по кругу, пока клиент его не закрыл, без
`Connection: close`. Проверьте `curl -v http://localhost:8088/ http://localhost:8088/orders` —
какая строка говорит, что второй request ушёл по тому же connection'у? Повторите поломку шага 3
с `using` на месте: почему `curl` снова ждёт?

**Приёмка:** принимающий шлёт вашему серверу request'ы через `curl` и может попросить сломать строку
response'а и заранее сказать, что увидит клиент. На следующей лабораторной этот же сервис
соберётся на ASP.NET Core, и его response'ы сравнят с этими.
