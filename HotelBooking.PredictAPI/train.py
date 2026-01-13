import pandas as pd
import joblib
from sklearn.model_selection import train_test_split
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import OneHotEncoder
from sklearn.metrics import mean_absolute_error
from sklearn.ensemble import HistGradientBoostingRegressor

DATA_PATH = "data/hotel_bookings.csv"
OUT_PATH = "artifacts/adr_model.joblib"

df = pd.read_csv(DATA_PATH)

# ---- 1) Temizleme ----
# adr hedefimiz. Bazı satırlarda adr 0 veya aşırı uç olabilir, basitçe filtreleyelim
df = df[df["adr"].notna()]
df = df[(df["adr"] > 0) & (df["adr"] < 1000)]

# ---- 2) Feature engineering ----
# arrival_date_month string -> kategorik, aynı şekilde market_segment, customer_type
# stays_in_weekend_nights + stays_in_week_nights zaten nights gibi çalışır
# lead_time (kaç gün önce alındı) çok önemli demand sinyali

target = "adr"

categorical = [
    "hotel", "arrival_date_month", "meal", "market_segment",
    "distribution_channel", "customer_type", "deposit_type"
]

numeric = [
    "lead_time", "arrival_date_week_number", "arrival_date_day_of_month",
    "stays_in_weekend_nights", "stays_in_week_nights",
    "adults", "children", "babies",
    "is_repeated_guest", "previous_cancellations", "booking_changes",
    "required_car_parking_spaces", "total_of_special_requests"
]

# dataset sütunları değişebildiği için mevcut olanları seç
categorical = [c for c in categorical if c in df.columns]
numeric = [c for c in numeric if c in df.columns]

X = df[categorical + numeric].copy()
y = df[target].copy()

# ---- 3) Preprocess + Model ----
preprocess = ColumnTransformer(
    transformers=[
        ("cat", OneHotEncoder(handle_unknown="ignore"), categorical),
        ("num", "passthrough", numeric),
    ]
)

model = HistGradientBoostingRegressor(max_depth=6, random_state=42)

pipe = Pipeline(steps=[("prep", preprocess), ("model", model)])

X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=42
)

pipe.fit(X_train, y_train)

pred = pipe.predict(X_test)
mae = mean_absolute_error(y_test, pred)
print("MAE (EUR):", round(mae, 2))

joblib.dump(pipe, OUT_PATH)
print("Saved model to:", OUT_PATH)
