# BiyahePro Customer Mobile

Initial customer booking mobile scaffold for the existing BiyahePro ASP.NET API.

## Included in this scaffold
- Expo + TypeScript + Expo Router
- Customer registration and login
- Secure native JWT session persistence (`expo-secure-store`)
- Home / Bookings / Account tabs
- Fare estimate integration: `POST /api/trips/estimate`
- Customer booking integration: `POST /api/trips`
- Booking history integration: `GET /api/trips/history`
- API base URL configuration for Android emulator, iOS/web, and physical devices

## Run
```bash
npm install
npm start
```

The app uses Expo SDK 57 dependencies. For a physical phone, copy `.env.example` to `.env` and set `EXPO_PUBLIC_API_URL` to your development computer's LAN address, for example:

```env
EXPO_PUBLIC_API_URL=http://192.168.1.20:5000
```

Then make sure the ASP.NET API is reachable from other devices on your LAN. `localhost` on a phone refers to the phone itself, not your PC.

## Scaffold limitation / next step
The first booking screen intentionally uses editable latitude/longitude placeholders so the API contract is live immediately. The next implementation slice should add device location permission, a map, pickup/dropoff pin selection, reverse geocoding, vehicle type selection, and SignalR live trip status.


## Part 2 — Map-based booking flow

The booking screen now includes:

- Device foreground-location permission and current-position pickup
- Interactive map with pickup and destination markers
- Long-press map selection for either point
- Reverse geocoding for readable addresses
- Manual address editing
- Cash / GCash / card selection
- Server-side fare estimation
- Booking confirmation
- Searching-for-driver screen with trip-status polling

### Map setup

Copy `.env.example` to `.env` and set:

```text
EXPO_PUBLIC_API_URL=http://YOUR-PC-LAN-IP:5000
EXPO_PUBLIC_GOOGLE_MAPS_API_KEY=YOUR_ANDROID_GOOGLE_MAPS_KEY
```

For Expo Go testing, the map can be tested without configuring a production Google Maps key. Store/development builds require the appropriate Google Maps configuration.

Run:

```bash
npm install
npm start
```
