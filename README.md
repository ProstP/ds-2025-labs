# PA3. Модель передачи сообщений.

**Цель:** научиться использовать брокер сообщений для организации асинхронного вычисления и масштабирования РС (распределённой системы).

# Задание

>Задание делается на базе задания PA2. Вы можете использовать или не использовать docker и docker compose при выполнении задания.

Необходимо перенести вычисление оценки текста из веб-приложения и в отдельный фоновый процесс — новый компонент *RankCalculator*.

В роли брокера сообщений можно использовать один из двух на выбор:

1. RabbitMQ — см. пример в каталоге `rabbitmq_example/`
2. NATS — см. пример в каталоге `nats_example/`

Функциональные требования:

1. Компонент RankCalculator подключается в роли потребителя к брокеру сообщений и начинает выполнять задания, отправляемые в его очередь в виде сообщений.
2. Значение вычисленного ранга должно сохраняться в общей базе данных Redis, как и прежде.

Нефункциональные требования:

1. Обеспечить возможность запуска нескольких процессов RankCalculator так, чтобы сообщения распределялись между экземплярами процессов, т.е. чтобы процессы работали в режиме конкурирующих потребителей
2. Добавить запуск/останов экземпляров *RankCalculator* в скрипты start/stop либо в `docker-compose.yaml`

# Уточнения

## Изменения в веб-приложении

Обработчик страницы *index* теперь не должен вычислять значение rank. Вместо этого должно создаваться задание для процесса *RankCalculator* и отправляться через брокер сообщений NATS. Остальная логика остаётся без изменений - пользователю возвращается код *302 Redirect* на страницу *summary*.

## Какую информацию передавать в сообщениях?

Хорошим подходом считается сохранять большие данные в хранилище сразу после получения от пользователя, а в сообщениях передавать ссылку, по которой можно получить данные при необходимости. Но есть брокеры сообщений, которые спроектированы специально для передачи больших объёмов данных внутри сообщения (например Apache Kafka).

### Ссылки:

Модель работы:

1. Описание шаблона "Конкурирующие потребители": [Competing Consumers pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/competing-consumers)

Брокер RabbitMQ:

1. [RabbitMQ Tutorials](https://www.rabbitmq.com/tutorials)
2. Библиотека для работы с RabbitMQ: [RabbitMQ.Client](https://www.nuget.org/packages/RabbitMQ.Client)
3. Документация для C#: [.NET/C# Client API Guide](https://www.rabbitmq.com/client-libraries/dotnet-api-guide)

Брокер NATS:

1. Документация: [Developing With NATS](https://docs.nats.io/using-nats/developer)
2. Документация: [NATS and Docker](https://docs.nats.io/running-a-nats-service/nats_docker)
3. Библиотека для работы с NATS: [NATS.Client](https://www.nuget.org/packages/NATS.Client)
