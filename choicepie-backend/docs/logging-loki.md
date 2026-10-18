# 後端 Log：Serilog → Loki

後端用 Serilog 輸出到 stdout，**app 本身不知道 Loki 在哪**。由 Grafana Alloy 讀取容器的 stdout，解析後推到 Loki。
本文件說明 log 的格式，並附上一份 Alloy 設定範例。Metrics（Prometheus）和 tracing（Tempo）還沒做，這裡不涵蓋。

## 輸出格式

| 環境 | 格式 | 設定 |
|---|---|---|
| Development | 人類可讀的單行文字 | `LogOutput:Format` 留空 |
| 其他 | 每筆一行 JSON | `LogOutput:Format` 留空 |
| 任何環境強制指定 | `Text` / `Json` | `LogOutput:Format`，或環境變數 `LogOutput__Format` |

等級寫在 `appsettings.json` 的 `Serilog:MinimumLevel`，可以用環境變數覆寫，例如 `Serilog__MinimumLevel__Override__Microsoft.EntityFrameworkCore=Information`。

JSON 每一行長這樣：

```json
{"@t":"2026-10-18T04:56:41.9000000+00:00","@l":"Information","@m":"HTTP GET /api/v1/quizzes/42 responded 200 in 12.3 ms","@tr":"b557b1b60b3a22d19b03dbc195c9552b","@sp":"8cf0b20646dd61be","RequestMethod":"GET","RequestPath":"/api/v1/quizzes/42","StatusCode":200,"Elapsed":12.3,"RoutePattern":"api/v{version:apiVersion}/quizzes/{id}","ClientIp":"203.0.113.7","UserId":"…","SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware","Application":"ChoicePie.Backend.WebApi","Environment":"Production","MachineName":"choicepie-api-1","Version":"1.0.0+abc123"}
```

| 欄位 | 說明 |
|---|---|
| `@t` | 時間戳（RFC 3339，含時區位移） |
| `@l` | 等級（`Verbose`/`Debug`/`Information`/`Warning`/`Error`/`Fatal`），一定存在 |
| `@m` | 已組好的訊息，純文字 |
| `@x` | 例外的 stack trace（有例外時才出現） |
| `@tr` / `@sp` | W3C TraceId / SpanId。之後接 Tempo 時，Grafana 用它從 log 跳到 trace |
| `Application` / `Environment` / `MachineName` / `Version` | 每筆都有。`MachineName` 是容器 hostname，用來分辨是哪個節點 |
| `SourceContext` | 發出 log 的類別 |

每個 HTTP request 結束時會記一行摘要（`SourceContext = Serilog.AspNetCore.RequestLoggingMiddleware`），額外帶這些欄位：`RequestMethod`、`RequestPath`、`RoutePattern`、`StatusCode`、`Elapsed`、`ClientIp`、`RequestHost`、`UserId`（已登入時才有）。
- `/health/*` 不記錄。
- 5xx 只記 Warning，因為例外本身已經由 `DefaultExceptionHandler` 以 Error 記錄並送往 Sentry。
- SignalR 的 hub 方法呼叫不會逐筆記錄。

**PII**：只記 id（`UserId`、`MemberId`…）和 `ClientIp`，絕不記 email、名稱、token、密碼或 request/response body。新增 log 時請遵守。

## Loki label 原則

只有**低基數**的欄位才能當 label：`app`、`env`、`level`。
`MachineName`、`UserId`、`TraceId`、`RoomCode`、`RequestPath` 這些都留在 JSON 欄位裡，查詢時用 `| json` 取出。
高基數的 label 會讓 Loki 的 index 和 stream 數量暴增。

## Alloy 設定範例

假設 Alloy 跟後端跑在同一台 Docker host，compose 的 service 名稱是 `choicepie.backend.webapi`：

```alloy
discovery.docker "containers" {
  host = "unix:///var/run/docker.sock"
}

discovery.relabel "choicepie_backend" {
  targets = discovery.docker.containers.targets

  // 只收後端容器。
  rule {
    source_labels = ["__meta_docker_container_label_com_docker_compose_service"]
    regex         = "choicepie.backend.webapi"
    action        = "keep"
  }

  rule {
    target_label = "app"
    replacement  = "choicepie-backend"
  }
}

loki.source.docker "choicepie_backend" {
  host       = "unix:///var/run/docker.sock"
  targets    = discovery.relabel.choicepie_backend.output
  forward_to = [loki.process.choicepie_backend.receiver]
}

loki.process "choicepie_backend" {
  stage.json {
    expressions = {
      level = "\"@l\"",
      ts    = "\"@t\"",
      env   = "Environment",
    }
  }

  stage.labels {
    values = {
      level = "",
      env   = "",
    }
  }

  stage.timestamp {
    source = "ts"
    format = "RFC3339Nano"
  }

  forward_to = [loki.write.default.receiver]
}

loki.write "default" {
  endpoint {
    url = "http://loki:3100/loki/api/v1/push"
  }
}
```

在 Kubernetes 上改用 `discovery.kubernetes` + `loki.source.kubernetes`，`loki.process` 這段不用改。

## 常用查詢（LogQL）

Loki 的 `| json` 會把欄位名稱裡的 `@` 換成 `_`，所以 `@tr` 要寫成 `_tr`。

```logql
# 某個 request 在所有節點上的完整 log
{app="choicepie-backend"} | json | _tr="b557b1b60b3a22d19b03dbc195c9552b"

# 5xx 依端點統計
sum by (RoutePattern) (count_over_time({app="choicepie-backend"} | json | StatusCode >= 500 [5m]))

# 某個端點的 p95 延遲
quantile_over_time(0.95, {app="choicepie-backend"} | json | RoutePattern="api/v{version:apiVersion}/quizzes/{id}" | unwrap Elapsed [5m]) by (MachineName)

# 某個節點的錯誤
{app="choicepie-backend", level=~"Error|Fatal"} | json | MachineName="choicepie-api-1"

# Outbox 的 dead letter
{app="choicepie-backend", level="Error"} | json | SourceContext=~".*OutboxProcessor"
```
