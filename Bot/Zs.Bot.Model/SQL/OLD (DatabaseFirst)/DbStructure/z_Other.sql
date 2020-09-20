
SELECT u."UserFirstName", u."UserLastName", count(m.*) "MessageCount"
FROM rmgr."ReceivedMsg" m
JOIN rmgr."User" u ON u."UserId" = m."UserId"
WHERE m."InsertDate" > '2018-11-02 00:00:15'
  AND m."ChatId" = -1001364555739
  --AND NOT u."RoleName" = 'Owner' 
  --AND NOT u."RoleName" = 'Administrator' 
GROUP BY m."UserId", u."UserFirstName", u."UserLastName"
ORDER BY count(m.*) DESC






