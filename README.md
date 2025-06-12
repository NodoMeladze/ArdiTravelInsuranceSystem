ArdiTravel Insurance Microservices System

აღწერა:

მოგზაურობის დაზღვევის პოლისებისა და გადახდების მართვის სისტემა,
რომელიც აჩვენებს მიკროსერვისების არქიტექტურას სერვისებს შორის კომუნიკაციით, ვალიდაციით და ბიზნეს ლოგიკის გამიჯვნით.

PaymentService (Port 5000): გადახდის დამუშავება
PolicyService (Port 5001): პოლისების შექმნა და მენეჯმენტი
Communication: HTTP-based REST APIs,  circuit breaker-ის pattern-ის გამოყენებით

ძირითადი მახასიათებლები:

Pre-payment flow
პოლისის გააქტიურებამდე მომსახურების გადახდების დადასტურება
გადახდების უნიკალურობის შენარჩუნების მექანიზმი
ბიზნესზე მორგებული ვალიდაციები და პრემიუმ პაკეტის ხარჯის გამოთვლა
გლობალური ერორჰენდლინგი და ლოგირება

API Workflow-ს ინსტრუქცია
1. GET /api/policies/quote
2. POST /api/payments
3. POST /api/policies             



PaymentService: http://localhost:5000
PolicyService: http://localhost:5001



სატესტო რექუესთები:

Get Quote:
jsonPOST http://localhost:5001/api/policies/quote
{
  "destination": "Europe",
  "tripStartDate": "2025-07-01",
  "tripEndDate": "2025-07-14",
  "coverageType": 2
}

Process Payment:
jsonPOST http://localhost:5000/api/payments
{
  "amount": 130.00,
  "currency": "GEL",
  "paymentMethod": 1,
  "cardNumber": "4111111111111111",
  "cardHolderName": "John Doe"
}

Create Policy:
jsonPOST http://localhost:5001/api/policies
{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "destination": "Europe",
  "tripStartDate": "2025-07-01",
  "tripEndDate": "2025-07-14",
  "coverageType": 2,
  "paymentId": "{payment-id-from-step-2}"
}

!!! მიმდევრობას აქვს მნისნველობა !!!

პირველ რიგში Get Quote მეთოდით ხდება პოლისის ფასი გარკვევა, შემდეგ Process Payment-ით ხდება გადახდა და ბოლოს, 
Create Policy-ით ხდება პოლისის დამატება. 

Technology Stack

.NET 8, ASP.NET Core Web API
Entity Framework Core (In-Memory) // გამოყენებულია პოლისის მიკროსერვისში
Dapper (SQLite for PaymentService) // გამოყენებულია ფეიმენთის მიკროსერვისში
Serilog, Circuit Breaker Pattern
Domain-Driven Design principles