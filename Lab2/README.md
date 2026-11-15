# Laboratorul 2 - Sabloane structurale (sistem de livrare a coletelor)

## Scopul laboratorului

Scopul laboratorului este studierea sabloanelor de proiectare structurale si folosirea lor pentru a extinde aplicatia de livrare a coletelor din Laboratorul 1. Au fost implementate trei sabloane: **Adapter**, **Decorator** si **Facade**.

Functionalitatile noi adaugate sunt:
- un serviciu de livrare `international`, oferit de un curier partener cu un API diferit (**Adapter**);
- optiuni extra pentru colet - asigurare si manipulare pentru colete fragile - si un jurnal al notificarilor (**Decorator**);
- o interfata simpla prin care clientul trimite un colet fara sa cunoasca detaliile sistemului (**Facade**).

## Ce sunt sabloanele structurale

Sabloanele structurale descriu cum se combina clasele si obiectele pentru a forma structuri mai mari, pastrand codul flexibil si usor de extins. Ele ajuta la conectarea componentelor incompatibile, la adaugarea de comportament fara modificarea claselor existente si la ascunderea complexitatii unui subsistem.

## Cum se ruleaza

```powershell
cd Lab2
dotnet run
```

Programul cere datele destinatarului, greutatea, serviciul (`standard`, `express`, `overnight`, `international`), daca coletul este fragil si valoarea declarata pentru asigurare (0 = fara asigurare).

Exemplu:

```text
Numele destinatarului: Ion Popescu
E-mail destinatar: ion@example.com
Strada: str. Stefan cel Mare 10
Oras: Chisinau
Greutate (kg): 3,2
Serviciu (standard / express / overnight / international): express
Colet fragil? (da / nu): da
Valoare declarata pentru asigurare (lei, 0 = fara asigurare): 1000
```

Pretul se calculeaza astfel: express = 2 x (5 + 1,5 x 3,2) = 19,60 lei; asigurare = 2% x 1000 = 20 lei; fragil = 10 lei. Total: **49,60 lei**.

## Implementarea sabloanelor

### 1. Adapter

**Fisier:** `Adapter.cs`

Clasa `PartnerCourierApi` simuleaza API-ul unui curier extern. Interfata lui este incompatibila cu `IPricingStrategy`: primeste greutatea in grame si un cod de zona si intoarce pretul in eurocenti.

`PartnerPricingAdapter` implementeaza `IPricingStrategy` si traduce apelul: transforma kilogramele in grame, apeleaza API-ul si converteste eurocentii in lei.

```csharp
public double CalculatePrice(double weightKg)
{
    int grams = (int)Math.Ceiling(weightKg * 1000);
    int cents = api.QuoteInEuroCents(grams, zoneCode);
    return Math.Round(cents / 100.0 * LeiPerEuro, 2);
}
```

**Motivatie:** serviciul extern poate fi folosit ca orice alta strategie de pret. `DeliveryService` nu a fost modificat si nu stie ca in spate se afla un sistem strain.

### 2. Decorator

**Fisier:** `Decorator.cs`

`PricingDecorator` implementeaza `IPricingStrategy` si contine o alta strategie, careia ii delega calculul. Decoratorii concreti adauga cate un cost:
- `InsuranceDecorator` - 2% din valoarea declarata, minim 3 lei;
- `FragileDecorator` - taxa fixa de 10 lei.

Decoratorii se pot combina in orice ordine si peste orice serviciu:

```csharp
IPricingStrategy pricing = new ExpressPricing(new StandardPricing());
pricing = new InsuranceDecorator(pricing, 1000);
pricing = new FragileDecorator(pricing);
```

Acelasi sablon este folosit si pentru notificari: `NotifierDecorator` implementeaza `INotifier`, iar `LoggingNotifier` salveaza fiecare mesaj, cu ora, intr-un jurnal, apoi il trimite mai departe catre notificatorul decorat (de exemplu `ConsoleNotifier`).

**Motivatie:** optiunile extra se adauga la rulare, fara cate o subclasa pentru fiecare combinatie (express + fragil, overnight + asigurare + fragil etc.) si fara modificarea strategiilor existente.

### 3. Facade

**Fisier:** `Facade.cs`

`ShippingFacade` ascunde intregul subsistem: configurarea serviciilor (inclusiv adapterul), compunerea decoratorilor, `DeliveryService`, `LabelPrinter` si `LoggingNotifier`. Clientul foloseste doar cateva metode:
- `Ship(ShipmentRequest)` - aplica optiunile extra, inregistreaza coletul si afiseaza eticheta;
- `DeliverToEnd(trackingId)` - trece coletul prin toate etapele pana la `Delivered`;
- `ServiceNames` si `NotificationHistory` - pentru afisare.

```csharp
var shipping = new ShippingFacade();
Parcel parcel = shipping.Ship(new ShipmentRequest(
    name, email, street, city, weight, service, fragile, declaredValue));
shipping.DeliverToEnd(parcel.TrackingId);
```

**Motivatie:** `main.cs` se ocupa doar de citirea datelor si afisare. Daca se schimba modul de construire a serviciilor, se modifica doar fatada.

## Modificari aduse codului din Laboratorul 1

`DeliveryService` a primit o supraincarcare a metodei `Register` care accepta direct o `IPricingStrategy`. Astfel, fatada poate trimite o strategie decorata. Metoda veche (dupa numele serviciului) a ramas si foloseste noua metoda intern.

## Structura proiectului

- `SRP.cs`, `OCP.cs`, `DIP.cs` - codul preluat din Laboratorul 1 (`../Lab1`);
- `Adapter.cs` - `PartnerCourierApi`, `PartnerPricingAdapter`;
- `Decorator.cs` - `PricingDecorator`, `InsuranceDecorator`, `FragileDecorator`, `NotifierDecorator`, `LoggingNotifier`;
- `Facade.cs` - `ShipmentRequest`, `ShippingFacade`;
- `main.cs` - punctul de intrare, care foloseste fatada.

## Verificare

Se ruleaza aplicatia si se verifica:
- pretul pentru `international` (3,2 kg -> 250,38 lei);
- pretul cu asigurare si/sau fragil adaugat peste fiecare serviciu;
- afisarea optiunilor alese pe eticheta (ex. `express + asigurare + fragil`);
- jurnalul notificarilor la finalul programului;
- respingerea unui serviciu inexistent.

## Concluzie

Sabloanele structurale au permis extinderea aplicatiei fara modificari majore in codul existent. Adapterul a integrat un serviciu extern incompatibil, decoratorii au adaugat optiuni si jurnalizare care se pot combina liber, iar fatada a simplificat utilizarea intregului sistem din partea clientului.
