| Endpoint | Method | Auth used | Request body if any | HTTP status | Result summary | Failure reason if failed | Cleanup performed |
|---|---|---|---|---|---|---|---|
| /api/User/Login | POST | None | `{\"password\":\"user050426\",\"username\":\"user050426\"}` | 200 | Success | None |  |
| /api/Event | GET | Bearer | None | 200 | Success | None |  |
| /api/Event/Search | GET | Bearer | None | 200 | Success | None |  |
| /api/Event/{id} | GET | Bearer | None | 200 | Success | None |  |
| /api/Event/{id}/Performers | GET | Bearer | None | 200 | Success | None |  |
| /api/Logs/count | GET | Bearer | None | 200 | Success | None |  |
| /api/Logs/get/5 | GET | Bearer | None | 200 | Success | None |  |
| /api/Performer | GET | Bearer | None | 200 | Success | None |  |
| /api/Performer/{id} | GET | Bearer | None | 200 | Success | None |  |
| /api/Performer | POST | Bearer | `{\"name\":\"Temp Performer Test\",\"bio\":\"Test bio\"}` | 200 | Success | None |  |
| /api/Performer/{id} | PUT | Bearer | `{\"bio\":\"Updated bio\",\"name\":\"Updated Performer Test\",\"id\":6}` | 200 | Success | None |  |
| /api/Event | POST | Bearer | `{\"name\":\"Temp Event Test\",\"eventTypeId\":1,\"description\":\"Test description\",\"capacity\":100,\"endTime\":\"2026-05-08T22:56:46Z\",\"startTime\":\"2026-05-07T22:56:46Z\",\"location\":\"Test location\"}` | 200 | Success | None |  |
| /api/Event/{id} | PUT | Bearer | `{\"startTime\":\"2026-05-07T22:56:46Z\",\"id\":8,\"eventTypeId\":1,\"location\":\"Updated location\",\"name\":\"Updated Event Test\",\"description\":\"Updated description\",\"endTime\":\"2026-05-08T22:56:46Z\",\"capacity\":150}` | 200 | Success | None |  |
| /api/Event/{id}/Performers (PATCH) | PATCH | Bearer | `{\"performerId\":1}` | 200 | Success | None |  |
| /api/Event/{id}/Performers/{performerId} | DELETE | Bearer | None | 200 | Success | None |  |
| /api/Registration | GET | Bearer | None | 200 | Success | None |  |
| /api/Registration/{id} | GET | Bearer | None | 200 | Success | None |  |
| /api/Registration/My | GET | Bearer | None | 400 | Failed | `{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"id":["The value 'My' is not valid."]},"traceId":"00-b68bf393bd942f95a4468a293ca117ca-033acd9042127375-00"}` |  |
| /api/Registration | POST | Bearer | `None` | 0 | Skipped | `No safe cleanup endpoint (DELETE /api/Registration does not exist)` |  |
| /api/Performer/{id} | DELETE | Bearer | None | 200 | Success | None | Deleted temporary Performer 6 |
| /api/Performer/{id} | GET | Bearer | None | 404 | Failed | `Performer with id=6 was not found.` | Verified deletion (404) |
| /api/Event/{id} | DELETE | Bearer | None | 200 | Success | None | Deleted temporary Event 8 |
| /api/Event/{id} | GET | Bearer | None | 404 | Failed | `Event not found.` | Verified deletion (404) |

