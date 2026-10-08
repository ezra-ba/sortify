#inf

# Fragenkatalog JavaScript und Ereignisse

## Zu welchem Zweck wurde JavaScript geschaffen?

JavaScript wurde geschaffen, um Webseiten im Browser dynamisch und interaktiv zu machen. Benutzeraktionen wie Klicks, Tastatureingaben oder Formulareingaben können verarbeitet werden, ohne dass für jede Reaktion eine neue Webseite vom Server geladen werden muss.

## Was ist JavaScript?

JavaScript ist eine Programmiersprache, die im Browser ausgeführt werden kann. Sie ermöglicht unter anderem, HTML-Elemente über das Document Object Model (DOM) zu lesen und zu verändern, Benutzereingaben zu verarbeiten und auf Ereignisse zu reagieren.

## Wie stellt sich die Ausführungsreihenfolge von JavaScript dar?

JavaScript-Anweisungen werden grundsätzlich in der Reihenfolge ausgeführt, in der sie im Skript stehen. Funktionen werden jedoch erst ausgeführt, wenn sie aufgerufen werden. Bei ereignisgesteuertem JavaScript wird eine Funktion erst dann aufgerufen, wenn das zugehörige Ereignis eintritt, zum Beispiel ein Klick oder das Laden der Webseite.

Beim Zugriff auf DOM-Elemente muss außerdem darauf geachtet werden, dass der DOM-Baum bereits aufgebaut ist. Deshalb werden solche Zugriffe häufig in eine Initialisierungsfunktion innerhalb von `window.onload` geschrieben.

## Was ist eine anonyme Funktion?

Eine anonyme Funktion ist eine Funktion ohne eigenen Namen. Sie kann beispielsweise direkt einem Eventhandler zugewiesen werden:

```javascript
document.onclick = function () {
  // Anweisungen
};
```

## Wo macht die Verwendung von anonymen Funktionen Sinn?

Anonyme Funktionen sind sinnvoll, wenn eine Funktion nur an einer Stelle benötigt wird, zum Beispiel als Eventhandler. Sie können direkt dort definiert werden, wo sie verwendet werden, und müssen nicht zusätzlich über einen Funktionsnamen angesprochen werden. Die PDF nennt die Verwendung anonymer Funktionen bei Ereignisbehandlungen als üblichen Praxisfall.

## Beschreiben Sie folgenden Code `window.onload = function() { ... }`.

`window.onload` ist der Eventhandler für das vollständige Laden der Webseite. Ihm wird eine anonyme Funktion zugewiesen. Diese Funktion wird erst ausgeführt, wenn die Webseite geladen und der DOM-Baum aufgebaut wurde.

```javascript
window.onload = function () {
  // DOM-Elemente können hier sicher angesprochen werden
};
```

Dadurch werden Zugriffe auf Elemente wie `document.getElementById(...)` nicht zu früh ausgeführt. Die PDF weist außerdem darauf hin, dass die Schreibweise ohne Klammern verwendet wird: `window.onload = function () { ... };`. Mit `window.onload()` würde man die Funktion unmittelbar aufrufen, statt sie als Eventhandler zu registrieren.

## Wie kann man das Ereigniseventobjekt nach dem vom W3C standardisierten Standardmodell verwenden bzw. ansprechen?

Im standardisierten Ereignismodell wird das Ereignisobjekt als erster Parameter an die Ereignisfunktion übergeben. Der Parameter kann beispielsweise `evt` oder `event` heißen:

```javascript
document.getElementsByTagName("h1")[0].onclick = function (evt) {
  // evt enthält Informationen zum Ereignis
  console.log(evt.type);
};
```

Bei einer benannten Funktion wird das Ereignisobjekt ebenfalls als erster Parameter übernommen:

```javascript
function ereignisfunktion(evt) {
  console.log(evt.type);
}

document.getElementsByTagName("h1")[0].onclick = ereignisfunktion;
```

Über das Ereignisobjekt kann man je nach Ereignis zusätzliche Informationen abfragen, zum Beispiel die verwendete Maustaste, gedrückte Zusatztasten, die Klickkoordinaten oder die gedrückte Taste.

