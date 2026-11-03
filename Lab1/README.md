# Laboratorul 1 - Principii SOLID (sistem de livrare a coletelor)

## Scopul laboratorului

Scopul laboratorului este analiza principiilor SOLID si implementarea a trei dintre ele intr-o aplicatie simpla pentru livrarea coletelor. Principiile alese sunt SRP (Single Responsibility Principle), OCP (Open/Closed Principle) si DIP (Dependency Inversion Principle).

Aplicatia permite inregistrarea unui colet cu destinatar, adresa, greutate si serviciu de livrare (standard, express sau overnight). Programul calculeaza pretul, afiseaza eticheta si parcurge coletul prin etapele de livrare, trimitand o notificare clientului la fiecare schimbare.

## Ce reprezinta SOLID

SOLID este un set de cinci principii folosite in programarea orientata pe obiecte pentru a obtine cod mai organizat, mai usor de inteles si mai simplu de modificat. Principiile ajuta la reducerea dependintelor dintre clase si la separarea responsabilitatilor.

In cadrul acestui laborator nu au fost implementate toate cele cinci principii. Au fost selectate SRP, OCP si DIP deoarece se potrivesc natural cu domeniul livrarii: exista date despre colet, mai multe servicii de livrare cu preturi diferite si mai multe modalitati de a notifica clientul.

## Cum se ruleaza

Este necesar sa fie instalat .NET SDK. Proiectul foloseste .NET 10.

Din PowerShell se ruleaza:

```powershell
cd D:\TMPS\lab1
dotnet run
```

Pentru verificarea compilarii fara pornirea programului:

```powershell
dotnet build
```

### Utilizarea programului

Programul cere pe rand numele destinatarului, e-mailul, strada, orasul, greutatea (in kg) si serviciul. Serviciile acceptate sunt `standard`, `express` si `overnight`. Greutatea trebuie sa fie intre 0 si 30 kg.

La final, programul afiseaza eticheta coletului, apoi il trece prin etapele `ridicat`, `in tranzit` si `livrat`, cu cate o notificare pentru fiecare etapa.

Un exemplu de interactiune:

```text
Numele destinatarului: Ion Popescu
E-mail destinatar: ion@example.com
Strada: str. Stefan cel Mare 10
Oras: Chisinau
Greutate (kg): 3,2
Serviciu (standard / express / overnight): express
```

Pentru acest exemplu, pretul standard este 5 + 1,5 x 3,2 = 9,80 lei, iar serviciul express dubleaza pretul, deci suma finala este 19,60 lei.

## Implementarea principiilor SOLID

### 1. SRP - Single Responsibility Principle

SRP inseamna ca o clasa trebuie sa aiba o singura responsabilitate principala si un singur motiv important pentru a fi modificata.

In proiect, SRP este implementat in `SRP.cs`. Clasa `Parcel` retine datele coletului, clasa `Address` retine adresa, iar `LabelPrinter` are responsabilitatea de a afisa eticheta. Clasa `Parcel` nu calculeaza pretul si nu afiseaza nimic.

Responsabilitatile sunt separate: daca se schimba layout-ul etichetei, se modifica doar `LabelPrinter`; daca se schimba datele unui colet, se modifica doar `Parcel`.

### 2. OCP - Open/Closed Principle

OCP inseamna ca un modul trebuie sa fie deschis pentru extindere, dar inchis pentru modificari. Astfel, putem adauga comportamente noi fara sa schimbam codul existent.

In `OCP.cs` exista interfata `IPricingStrategy`, care defineste metoda `CalculatePrice(double weightKg)`. Aceasta este implementata de `StandardPricing` (5 lei + 1,50 lei/kg), `ExpressPricing` (dublul pretului standard) si `OvernightPricing` (pretul standard plus 15 lei).

Pentru a adauga un serviciu nou, de exemplu `EconomyPricing`, se scrie o clasa noua care implementeaza `IPricingStrategy` si se adauga o linie in dictionarul din `main.cs`. Clasa `DeliveryService` nu se modifica.

### 3. DIP - Dependency Inversion Principle

DIP inseamna ca modulele de nivel inalt nu trebuie sa depinda de detalii concrete, ci de abstractii.

In `DIP.cs`, clasa `DeliveryService` (logica de business) primeste prin constructor un `INotifier` si un dictionar de `IPricingStrategy`. Nu stie daca mesajele ajung in consola sau pe e-mail. In `main.cs` se decide ce implementari se folosesc (`ConsoleNotifier`, `StandardPricing` etc.), iar `InMemoryNotifier` poate fi folosit in locul consolei pentru verificari.

## Structura proiectului

Fisierul `main.cs` este punctul de intrare si contine fluxul interactiv al aplicatiei. `SRP.cs` contine clasele pentru colet, adresa si eticheta, `OCP.cs` contine strategiile de pret, iar `DIP.cs` contine interfata de notificare si `DeliveryService`. Configuratia proiectului .NET se afla in `TMPS.csproj`.

Fluxul aplicatiei porneste din `main.cs`: datele sunt citite de la utilizator, `DeliveryService.Register` calculeaza pretul si salveaza coletul, `LabelPrinter` afiseaza eticheta, iar `DeliveryService.Advance` trece coletul prin etape.

## Verificare si testare

Testarea se face prin rularea interactiva a aplicatiei. Trebuie verificate:
- pretul pentru fiecare serviciu (standard, express, overnight);
- respingerea unui serviciu inexistent si a unei greutati in afara intervalului 0-30 kg;
- ordinea etapelor: un colet nu poate sari o etapa si nu poate fi avansat dupa `livrat`.

Comanda `dotnet build` verifica daca fisierele C# se compileaza, iar `dotnet run` porneste aplicatia. Proiectul nu contine inca teste automate.

## Posibilitati de extindere

Pentru un serviciu nou de livrare se poate crea o clasa noua care implementeaza `IPricingStrategy`. Pentru un canal nou de notificare (e-mail, SMS) se poate crea o clasa care implementeaza `INotifier`.

Ca imbunatatiri viitoare, aplicatia ar putea salva coletele intr-un fisier sau intr-o baza de date, ar putea folosi `decimal` in loc de `double` pentru sume de bani si ar putea primi mai multe colete intr-o singura sesiune.

## Concluzie

Laboratorul arata cum principiile SOLID fac codul mai clar, mai usor de intretinut si mai simplu de extins. Separarea responsabilitatilor prin SRP, folosirea strategiilor de pret prin OCP si dependenta de abstractii prin DIP reduc legaturile dintre componente. Aplicatia poate fi extinsa cu servicii de livrare sau canale de notificare noi fara modificari majore in clasele existente.
